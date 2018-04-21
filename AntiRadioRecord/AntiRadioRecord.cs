using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace AntiRadioRecord
{
    class AntiRadioRecordWave
    {
        #region Fields
        public List<Song> SongsOnAir;

        public List<WikiArt.Painting> paintings;

        private RadioRecord radioRecord;

        private WikiArt wikiArt;

        private Random random;

        private vkmDownload download;

        private gateToDB gate;

        private bool _ReadyToPlay { get; set; }
        public bool ReadyToPlay
        {
            get
            {
                return _ReadyToPlay;
            }
        }

        private Action ChangeMusicInPlayer;
        #endregion

        #region Constructors
        public AntiRadioRecordWave(Action ChangeMusicInPlayer)
        {
            this.ChangeMusicInPlayer = ChangeMusicInPlayer;
            download = new vkmDownload();
            _ReadyToPlay = false;
            gate = new gateToDB();
            SongsOnAir = new List<Song>();
            radioRecord = new RadioRecord(getNewSong);
            random = new Random((int)DateTime.Now.ToBinary());
            LoadCovers();
            _ReadyToPlay = true;
        }
        #endregion

        #region Main Functions

        private async void LoadCovers()
        {
            await Task.Run(async () => 
            {
                wikiArt = new WikiArt();
                paintings = await wikiArt.GetArtistPaintings(new WikiArt.Artist("albert-bierstadt"));
            });
        }

        public async Task<byte[]> GetNewCover()
        {
            return await wikiArt.GetPaintingImage(paintings[random.Next(0, paintings.Count)]);
        }

        private void getNewSong()
        {
            Song currentSong = radioRecord.currentSong;
            if(checkSong(currentSong))
            {
                AddInSongsOnRadioList(currentSong, SongsOnAir, UpdateBufferOfSongs);
            }
            else
            {
                Task.Run(() => 
                {
                    currentSong = new Song(gate.GetRandomSong);
                    AddInSongsOnRadioList(currentSong, SongsOnAir, UpdateBufferOfSongs);
                });
            }
            if(SongsOnAir.Count == 1)
            {
                currentSong = new Song(gate.GetRandomSong);
                AddInSongsOnRadioList(currentSong, SongsOnAir, UpdateBufferOfSongs);
            }
        }

        public void SongOnRadioFinished()
        {
            SongsOnAir.RemoveAt(0);
            ChangeMusicInPlayer();
            UpdateBufferOfSongs();
        }

        private void UpdateBufferOfSongs()
        {
            if(SongsOnAir.Count > 0)
            {
                Parallel.For(0, (SongsOnAir.Count >= 2 ? 2 : 1), (i) => 
                {
                    if (SongsOnAir[i].SongFile == null)
                    {
                        SongsOnAir[i].SongFile = GetDownloadedSong(SongsOnAir[i].ToString());
                    }
                });
                if(SongsOnAir.Count == 1)
                {
                    ChangeMusicInPlayer();
                }
            }
        }

        public byte[] GetDownloadedSong(string SongName)
        {
            byte[] DownloadedSong = null;
            var task = Task.Run(async () => await download.GetMp3ForMusic7sAsync(SongName));
            task.Wait();
            DownloadedSong = task.Result;
            return DownloadedSong;
        }

        public Song GetCurrentSongOnRadioPlaying()
        {
            return SongsOnAir[0];
        }

        private bool checkSong(Song song)
        {
            if(song.artist.artistName!=null && song.songName!=null)
            {
                if(song.artist.artistName == "Record Dance Radio")
                {
                    return false;
                }
                if (Regex.IsMatch(song.songName, "([А-ЯЁ][а-яё]+)") || Regex.IsMatch(song.artist.artistName, "([А-ЯЁ][а-яё]+)") || SongsOnAir.Contains(song))
                {
                    return false;
                }
            }          
            return true;
        }

        private static void AddInSongsOnRadioList(Song song, List<Song> SongsOnAir , Action UpdateBuffer)
        {
            if(SongsOnAir.Contains(song) == false && song.artist.artistName != "Record Dance Radio")
            {
                lock (SongsOnAir)
                {
                    SongsOnAir.Add(song);
                    Thread.Sleep(100);
                    UpdateBuffer();
                }
            }
        }
        #endregion

    }
}
