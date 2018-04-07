using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AntiRadioRecord
{
    class AntiRadioRecordWave
    {
        #region Fields
        public List<Song> SongsOnAir;
        public List<WikiArt.Painting> paintings;
        private RadioRecord radioRecord;
        public event Changes changeMusic;
        public event Action onSongFinished;
        private WikiArt wikiArt;
        private Random random;
        public List<byte[]> BufferedSongs;
        private vkmDownload download;
        #endregion

        #region Constructors
        public AntiRadioRecordWave()
        {
            BufferedSongs = new List<byte[]>();
            BufferedSongs.Capacity = 2;
            SongsOnAir = new List<Song>();
            radioRecord = new RadioRecord(getNewSong);
            random = new Random((int)DateTime.Now.ToBinary());
            LoadCovers();
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

        private async void getNewSong()
        {
            Song currentSong = radioRecord.currentSong;
            if(checkSong(currentSong))
            {
                SongsOnAir.Add(currentSong);
                if(BufferedSongs.Count < BufferedSongs.Capacity)
                {
                    BufferedSongs.Add(await download.GetMp3Async(SongsOnAir[0].songName));
                }
                if(SongsOnAir.Count==1)
                {
                    changeMusic();
                }
            }
        }

        public async void ChangeMusic()
        {
            changeMusic();
            BufferedSongs[0] = await download.GetMp3Async(SongsOnAir[0].songName);
        }

        public void SongOnRadioFinished()
        {
            BufferedSongs.RemoveAt(0);
        }

        public byte[] GetCurrentSongOnRadioPlaying()
        {
            return BufferedSongs[0];
        }

        private bool checkSong(Song song)
        {
            if(song.artist.artistName!=null && song.songName!=null)
            {
                if (Regex.IsMatch(song.songName, "([А-ЯЁ][а-яё]+)") || Regex.IsMatch(song.artist.artistName, "([А-ЯЁ][а-яё]+)"))
                {
                    return false;
                }
            }          
            return true;
        }
        #endregion

    }
}
