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
        public event Action changeMusic;
        public event Action onSongFinished;
        private WikiArt wikiArt;
        private Random random;
        public List<byte[]> BufferedSongs;
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
        #endregion

        #region Constructors
        public AntiRadioRecordWave()
        {
            download = new vkmDownload();
            _ReadyToPlay = false;
            gate = new gateToDB();
            BufferedSongs = new List<byte[]>();
            BufferedSongs.Capacity = 2;
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
                SongsOnAir.Add(currentSong);
                UpdateBufferOfSongs();
                if (SongsOnAir.Count == 1)
                {
                    changeMusic();
                }
            }
            else
            {
                Task.Run(() => 
                {
                    currentSong = new Song(gate.GetRandomSong);
                    SongsOnAir.Add(currentSong);
                    UpdateBufferOfSongs();
                    if (SongsOnAir.Count == 1)
                    {
                        changeMusic();
                    }
                });
            }
        }

        public async void ChangeMusic()
        {
            changeMusic();
            BufferedSongs[0] = await download.GetMp3FromDownloadMusicVkAsync(SongsOnAir[0].songName);
        }

        public void SongOnRadioFinished()
        {
            BufferedSongs.RemoveAt(0);
            SongsOnAir.RemoveAt(0);
            UpdateBufferOfSongs();
        }

        private async void UpdateBufferOfSongs()
        {
            if (BufferedSongs.Count <= 1 && BufferedSongs.Count < BufferedSongs.Capacity)
            {
                BufferedSongs.Add(await download.GetMp3FromDownloadMusicVkAsync(SongsOnAir[0].songName));
            }
        }

        public byte[] GetCurrentSongOnRadioPlaying()
        {
            if(BufferedSongs.Count != 0)
            {
                return BufferedSongs[0];
            }
            else
            {
                return null;
            }
        }

        private bool checkSong(Song song)
        {
            if(song.artist.artistName!=null && song.songName!=null)
            {
                if (Regex.IsMatch(song.songName, "([А-ЯЁ][а-яё]+)") || Regex.IsMatch(song.artist.artistName, "([А-ЯЁ][а-яё]+)") || song.songName == "Record Dance Radio")
                {
                    return false;
                }
            }          
            return true;
        }
        #endregion

    }
}
