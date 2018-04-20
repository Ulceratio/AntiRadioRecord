using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data;
using System.IO;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;

namespace AntiRadioRecord
{

    #region Sql Commands Text
    class sqlCommands
    {               
        public static string createTrigger1
        {
            get
            {
                return "CREATE TRIGGER blockDuplicatesSongs" +
                    " ON Songs INSTEAD OF INSERT" +
                    " AS " +
                    " declare @songName varchar(max);" +
                    " declare @artistId int;" +
                    " declare @listen varchar(max);" +
                    " IF (select count(*) from Songs join inserted on Songs.songName=inserted.songName) = 0 " +
                    " BEGIN" +
                    " SELECT @songName = inserted.songName FROM inserted; " +
                    " SELECT @artistId = inserted.ArtistsId FROM inserted; " +
                    " SELECT @listen = inserted.listen FROM inserted; " +
                    " INSERT INTO Songs VALUES (@songName,null, @listen,@artistID);" +
                    " END " +
                    " ELSE " +
                    " BEGIN " +
                    " BEGIN RAISERROR ('Duplicate', 16, 1); " +
                    " RETURN END; " +
                    " END";
            }
        }

        public static string createTrigger2
        {
            get
            {
                return "CREATE TRIGGER blockDuplicatesArtists" +
                    " ON Artists INSTEAD OF INSERT" +
                    " AS declare @artistName varchar(max);" +
                    " IF (select count(*) from Artists join inserted on Artists.name = inserted.name) = 0 " +
                    " BEGIN" +
                    " SELECT @artistName = inserted.name FROM inserted; " +
                    " INSERT INTO Artists VALUES (@artistName,null,null,null);" +
                    " END " +
                    " ELSE " +
                    " BEGIN " +
                    " BEGIN RAISERROR ('Duplicate', 16, 1); " +
                    " RETURN END; " +
                    " END";
            }
        }

        public static string insertIntoArtists
        {
            get
            {
                return "CREATE PROCEDURE insertArtistIntoDB" +
                    " @artistName varchar(max), @artistID int output " +
                    " AS " +
                    " SET NOCOUNT ON; " +
                    " BEGIN TRANSACTION; " +
                    " BEGIN TRY " +
                    " INSERT INTO Artists VALUES (@artistName,null,null,null); " +
                    " IF @@TRANCOUNT > 0 " +
                    " COMMIT TRANSACTION;" +
                    " SET @artistID = @@IDENTITY;" +
                    " END TRY" +
                    " BEGIN CATCH" +
                    " IF @@TRANCOUNT > 0 " +
                    " SET @artistID = -1;" +
                    " ROLLBACK TRANSACTION;" +
                    " END CATCH;";
            }
        }

        public static string insertIntoSongs
        {
            get
            {
                return "CREATE PROCEDURE insertSongIntoDB" +
                    " @songName varchar(max)," +
                    " @artistID int ," +
                    " @listen varchar(max) ," +
                    " @songID int output " +
                    " AS " +
                    " SET NOCOUNT ON; " +
                    " BEGIN TRANSACTION;" +
                    " BEGIN TRY " +
                    " INSERT INTO Songs VALUES (@songName,null, @listen,@artistID);" +
                    " IF @@TRANCOUNT > 0 " +
                    " COMMIT TRANSACTION;" +
                    " SET @songID = @@IDENTITY; " +
                    " END TRY" +
                    " BEGIN CATCH " +
                    " IF @@TRANCOUNT > 0" +
                    " SET @songID = -1;" +
                    " ROLLBACK TRANSACTION; " +
                    " END CATCH;";
            }
        }

        public static string insertSimplifiedSongsIntoDb
        {
            get
            {
                return "CREATE PROCEDURE InsertSimplifiedSongsIntoDb" +
                    " @songName varchar(max), @listen varchar(max) " +
                    "AS" +
                    " SET NOCOUNT ON; " +
                    "BEGIN TRY " +
                    "INSERT INTO SimplifiedSongs values(@songName,@listen); " +
                    "END TRY " +
                    "BEGIN CATCH " +
                    "END CATCH;";
            }
        }

        public static string updateDays
        {
            get
            {
                return "CREATE PROCEDURE updateDay " +
                    "@dayID int" +
                    " AS " +
                    "SET NOCOUNT ON;" +
                    " update Days set Days.processed = 'true' where Days.id = @dayID";
            }
        }
    }
    #endregion

    public static class Chunk
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }

    public class gateToDB
    {
        #region DB objects
        public class Artists
        {
            [Key]
            public int ArtistsId { get; set; }
            public string name { get; set; }
            public string wikiLink { get; set; }
            public string soundCloudLink { get; set; }
            public string birth_date { get; set; }

            public ICollection<Songs> Songs { get; set; }
        }

        public class Songs
        {
            [Key]
            public int SongsId { get; set; }
            public string songName { get; set; }
            public string releaseDate { get; set; }
            public string listen { get; set; }

            public int ArtistsId { get; set; }
            public Artists Artists { get; set; }

        }

        public class Days
        {
            [Key]
            public int id { get; set; }
            public string day { get; set; }
            public string processed { get; set; }
        }

        public class SimplifiedSong
        {
            [Key]
            public int id { get; set; }
            public string SongName { get; set; }
            public string Listen { get; set; }
        }

        public class SongContext:DbContext
        {
            public SongContext() : base("DBConnection")
            {
                Database.SetInitializer<SongContext>(new CreateDatabaseIfNotExists<SongContext>());
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                .Entity<SimplifiedSong>()
                .Property(t => t.SongName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(
                new IndexAttribute("IX_SongName", 1) { IsUnique = true }));
            }

            public DbSet<Songs> songs { get; set; }
            public DbSet<Artists> artists { get; set; }
            public DbSet<Days> days { get; set; }
            public DbSet<SimplifiedSong> simplifiedSongs { get; set; }
        }
        #endregion

        #region Dates in str
        private string getEngNameOfMonth(int month)
        {
            switch (month)
            {
                case 1:
                    {
                        return "January".ToLower();
                    }
                case 2:
                    {
                        return "February".ToLower();
                    }
                case 3:
                    {
                        return "March".ToLower();
                    }
                case 4:
                    {
                        return "April".ToLower();
                    }
                case 5:
                    {
                        return "May".ToLower();
                    }
                case 6:
                    {
                        return "June".ToLower();
                    }
                case 7:
                    {
                        return "July".ToLower();
                    }
                case 8:
                    {
                        return "August".ToLower();
                    }
                case 9:
                    {
                        return "September".ToLower();
                    }
                case 10:
                    {
                        return "October".ToLower();
                    }
                case 11:
                    {
                        return "November".ToLower();
                    }
                case 12:
                    {
                        return "December".ToLower();
                    }
                default:
                    break;
            }
            return null;
        }
        private string getDateStr(DateTime date)
        {
            return date.ToString("dd") + "_" + getEngNameOfMonth(date.Month) + "_" + date.Year + "/";
        }
        #endregion

        #region Fields
        private int _numOfDays { get; set; }
        public int numOfDays
        {
            get
            {
                return (from element in context.days.AsParallel() where element.processed == "false" select element).Count();
            }
        }
        public bool isDbExist;
        private Action<string> writer;
        private Action increasePB;
        private Action closeFunction;
        public SongContext context;
        private Random random;
        public string GetRandomSong
        {
            get
            {
                using (SongContext songContext = new SongContext())
                {
                    var randomSong = songContext.simplifiedSongs.ToList()[(random.Next(0, songContext.simplifiedSongs.Count()-1))].SongName;
                    return randomSong;
                }
            }
        }
        #endregion

        #region Constructors
        public gateToDB()
        {
            context = new SongContext();
            random = new Random();
        }

        public gateToDB(Action<string> writer, Action increasePB , Action closeFunction)
        {
            random = new Random();
            this.writer = writer;
            this.increasePB = increasePB;
            this.closeFunction = closeFunction;
            context = new SongContext();
            if(!context.Database.Exists())
            {
                writer("Начато создание базы данных");
                context.Database.Create();
                writer("Начато создание триггеров");
                Thread.Sleep(1000);               
                createTrigger1();
                createTrigger2();
                writer("Начато создание вставок и обновлений");
                Thread.Sleep(1000);
                createInsertIntoSongs();
                createInsertIntoArtists();
                createUpdateDays();
                createInsertSimplifiedSongsIntoDb();
                writer("Начато заполнение дат до сегодняшнего дня");
                Thread.Sleep(1000);
                fillDatesUpToNow();
            }
            else
            {
                writer("Начато заполнение дат до сегодняшнего дня");
                Thread.Sleep(1000);
                fillDatesUpToNow();
            }
        }
        #endregion

        #region Triggers and Stored procedures
        private void createTrigger1()
        {
            context.Database.ExecuteSqlCommand(sqlCommands.createTrigger1);
        }

        private void createTrigger2()
        {
            context.Database.ExecuteSqlCommand(sqlCommands.createTrigger2);
        }

        private void createInsertIntoSongs()
        {
            context.Database.ExecuteSqlCommand(sqlCommands.insertIntoSongs);
        }

        private void createInsertSimplifiedSongsIntoDb()
        {
            context.Database.ExecuteSqlCommand(sqlCommands.insertSimplifiedSongsIntoDb);
        }

        private void createInsertIntoArtists()
        {
            context.Database.ExecuteSqlCommand(sqlCommands.insertIntoArtists);
        }

        private void createUpdateDays()
        {
            context.Database.ExecuteSqlCommand(sqlCommands.updateDays);
        }

        #endregion

        #region Inserts And Update
        /// <summary>
        /// Returns id of inserted artist,if artist already exists this function will return -1
        /// </summary>
        /// <param name="artist">name of inserted artist</param>
        /// <returns>Returns id of inserted artist</returns>
        public int InsertArtistIntoDb(Artist artist)
        {
            using (SongContext songContext = new SongContext())
            {
                var param1 = new SqlParameter
                {
                    ParameterName = "@artistName",
                    Value = artist.artistName,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.VarChar
                };

                var param2 = new SqlParameter
                {
                    ParameterName = "@artistID",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Int
                };

                try
                {
                    var resultOfQuery = songContext.Database.ExecuteSqlCommand("exec insertArtistIntoDB @artistName , @artistID OUTPUT", param1, param2);
                }
                catch
                { }

                return artist.artistId;
            }          
        }

        /// <summary>
        /// Returns id of inserted song,if song already exists this function will return -1
        /// </summary>
        /// <param name="song">Songs to insert</param>
        /// <param name="Listen">Is song need to be listened or not</param>
        /// <returns>Returns id of inserted song</returns>
        public int InsertSongIntoDb(Song song,string Listen)
        {
            using (SongContext songContext = new SongContext())
            {
                var param1 = new SqlParameter
                {
                    ParameterName = "@songName",
                    Value = song.songName,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.VarChar
                };

                var param2 = new SqlParameter
                {
                    ParameterName = "@artistID",
                    Value = song.artist.artistId,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.Int
                };

                var param3 = new SqlParameter
                {
                    ParameterName = "@listen",
                    Value = Listen,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.VarChar
                };

                var param4 = new SqlParameter
                {
                    ParameterName = "@songID",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Int
                };

                try
                {
                    var resultOfQuery = songContext.Database.ExecuteSqlCommand("EXEC insertSongIntoDB @songName, @artistID, @listen , @songID OUTPUT", param1, param2, param3, param4);
                }
                catch
                { }

                return song.songId;
            }          
        }

        private void UpdateDateInDb(Days days)
        {

            using (SongContext songContext = new SongContext())
            {
                var param1 = new SqlParameter
                {
                    ParameterName = "@dayID",
                    Value = days.id,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.Int
                };

                var resultOfQuery = songContext.Database.ExecuteSqlCommand("EXEC updateDay @dayID", param1);
            }           
        }

        public void InsertSimplifiedSongsIntoDb(List<SimplifiedSong> simplifiedSongs)
        {
            Parallel.ForEach(simplifiedSongs, song =>
            {
                using (SongContext songContext = new SongContext())
                {
                    var param1 = new SqlParameter
                    {
                        ParameterName = "@songName",
                        Value = song.SongName,
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.VarChar
                    };

                    var param2 = new SqlParameter
                    {
                        ParameterName = "@listen",
                        Value = song.Listen,
                        Direction = ParameterDirection.Input,
                        SqlDbType = SqlDbType.VarChar
                    };

                    try
                    {
                        var resultOfQuery = songContext.Database.ExecuteSqlCommand("EXEC InsertSimplifiedSongsIntoDb @songName, @listen", param1, param2);
                    }
                    catch
                    { }

                }
            });
            simplifiedSongs = null;
        }
        #endregion

        #region Gets
        public int GetArtistIdByName(Artist artist)
        {
            using (SongContext songContext = new SongContext())
            {
                var artistsWithSearchName = (from element in songContext.artists.AsParallel() where element.name == artist.artistName select element.ArtistsId).ToList();
                return artistsWithSearchName.Count == 0 ? -1 : artistsWithSearchName[0];
            }           
        }

        public int GetSongIdByName(Song song)
        {
            using (SongContext songContext = new SongContext())
            {
                var songsWithSearchName = (from element in songContext.songs.AsParallel() where element.songName == song.songName select element.SongsId).ToList();
                return songsWithSearchName.Count == 0 ? -1 : songsWithSearchName[0];
            }           
        }

        public int GetSongIdByName(string songName)
        {
            using (SongContext songContext = new SongContext())
            {
                var songsWithSearchName = (from element in songContext.songs.AsParallel() where element.songName == songName select element.SongsId).ToList();
                return songsWithSearchName.Count == 0 ? -1 : songsWithSearchName[0];
            }
        }

        #endregion

        #region Functions
        public void StartDump()
        {
            var days = (from element in context.days where element.processed == "false" select element).ToList();
            DumpSongsFromListAsync(days);
            //var res = Chunk.ChunkBy(days, days.Count / 4);
            //Parallel.ForEach(res, item =>
            //{
            //    DumpSongsFromListAsync(item);
            //});
        }


        public void fillDatesUpToNow()
        {
            string lastDate = "";
            try
            {
                lastDate = File.ReadAllText(Directory.GetCurrentDirectory() + "\\lastDate.txt");
            }
            catch
            { }
            var dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime date;
            if (lastDate == "")
            {
                date = new DateTime(2010, 11, 1);
            }
            else
            {
                date = Convert.ToDateTime(lastDate);
            }
            if (date != dtNow)
                while (date <= dtNow)
                {
                    string insertingDate = getDateStr(date);
                    context.days.Add(new Days() { day = insertingDate, processed = "false" });
                    File.WriteAllText(Directory.GetCurrentDirectory() + "\\lastDate.txt", date.ToString());
                    writer(date.ToString());
                    date = date.AddDays(1);
                }
            context.SaveChanges();
        }

        private async void DumpSongsFromListAsync(List<Days> days)
        {
            foreach (var Day in days)
            {
                pageParser data;
                data = new pageParser("http://www.moreradio.org/playlist_radio/radio_record_fm/" + Day.day);

                #region Simplified Downloader Whith Duplicates
                //using (SongContext songContext = new SongContext())
                //{
                //    using (SongList songList = new SongList(await data.getSongsAsync()))
                //    {
                //        if(!songList.IsIntraListNull)
                //        {
                //            context.simplifiedSongs.AddRange((from element in songList.ToList().AsParallel() select element.ToSimplifiedSong()).ToList());
                //            context.SaveChanges();
                //        }                                                
                //    }
                //}
                #endregion

                #region Simplified Downloader Whith Out Duplicates
                using (SongList songList = new SongList(await data.getSongsAsync()))
                {
                    if (!songList.IsIntraListNull)
                    {
                        InsertSimplifiedSongsIntoDb((from song in songList.ToList().AsParallel() select song.ToSimplifiedSong()).ToList());
                    }
                }                
                #endregion

                #region Main Downloader(Very heavy and very long in time)
                //if(songs!=null)
                //for (int i = 0; i < songs.Count; i++)
                //{
                //    if (songs[i].isRussianRetardedSong != true)
                //    {
                //        if (songs[i].artist.artistId == -1)
                //        {

                //            int id = InsertArtistIntoDb(songs[i].artist);
                //            if (id == -1)
                //            {
                //                songs[i].artist.getArtistId();
                //            }
                //            else
                //            {
                //                songs[i].artist.artistId = id;
                //            }
                //        }

                //        if (songs[i].artist.artistId != -1)
                //        {
                //            InsertSongIntoDb(songs[i], "true");
                //        }
                //    }
                //    else
                //    {
                //        if (songs[i].artist.artistId == -1)
                //        {

                //            int id = InsertArtistIntoDb(songs[i].artist);
                //            if (id == -1)
                //            {
                //                songs[i].artist.getArtistId();
                //            }
                //            else
                //            {
                //                songs[i].artist.artistId = id;
                //            }
                //        }

                //        if (songs[i].artist.artistId != -1)
                //        {
                //            InsertSongIntoDb(songs[i], "true");
                //        }
                //    }
                //}
                #endregion


                data = null;
                increasePB();
                UpdateDateInDb(Day);
                writer(Day.day);
                GC.Collect();
            }
            closeFunction();
        }
        #endregion
    }
}
