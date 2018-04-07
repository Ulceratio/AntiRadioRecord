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
        private RadioRecord radioRecord;
        public event Changes changeMusic;
        #endregion

        #region Constructors
        public AntiRadioRecordWave()
        {
            SongsOnAir = new List<Song>();
            radioRecord = new RadioRecord(getNewSong);
        }
        #endregion

        #region Main Functions
        private void getNewSong()
        {
            Song currentSong = radioRecord.currentSong;
            if(checkSong(currentSong))
            {
                SongsOnAir.Add(currentSong);
                if(SongsOnAir.Count==1)
                {
                    changeMusic();
                }
            }
        }

        public void ChangeMusic()
        {
            changeMusic();
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
