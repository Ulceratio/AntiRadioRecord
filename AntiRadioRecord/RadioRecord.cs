using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;

namespace AntiRadioRecord
{
    delegate void Changes();

    class RadioRecordSong
    {
        #region Fields
        public string time { get; set; }
        public string isodate { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        #endregion

        #region Constructors
        public RadioRecordSong()
        {
            time = "";
            isodate = "";
            artist = "";
            title = "";
            link = "";
        }

        public RadioRecordSong(string time, string isodate, string artist, string title , string link)
        {
            this.time = time;
            this.isodate = isodate;
            this.artist = artist;
            this.title = title;
            this.link = link;
        }
        #endregion

        public override string ToString()
        {
            return "time = " + time + " | artist = " + artist + " | title = " + title;
        }
    }

    class RadioRecord
    {
        #region Fields
        private string URL = "http://history.radiorecord.ru/index-json-formatted.php?station=rr"; // Record Dance Radio
        private HttpClient client;
        private HtmlParser parser;
        public event Changes OnSongChange;
        public Song currentSong;
        private System.Timers.Timer timer;
        #endregion

        #region Constructors
        public RadioRecord(Changes changes)
        {
            OnSongChange += changes;
            currentSong = null;
            client = new HttpClient();
            parser = new HtmlParser();
            //TickAsync(null);
            Task.Run(() => mainThread());
        }
        #endregion

        #region Main Functionality
        private void mainThread()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                Tick();
            };
            timer.Start();
        }

        private async Task<string> getJsonArrayAsync()
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync(URL);
        }

        private Song GetSong()
        {
            Song song = new Song();
            List<RadioRecordSong> resultOfDes = new List<RadioRecordSong>();
            Task t = Task.Run(async () => {
                string jsonArr = await getJsonArrayAsync();
                resultOfDes = JsonConvert.DeserializeObject<List<RadioRecordSong>>(jsonArr);
                song.songName = resultOfDes[0].title;
                song.artist.artistName = resultOfDes[0].artist;
            });
            t.Wait();
            resultOfDes = null;
            return song;
        }

        private void Tick()
        {
            Song song = null;
            try
            {
                song = GetSong();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if(currentSong != null)
            {
                if(song != null)
                {
                    if (currentSong != song)
                    {
                        currentSong = song;
                        OnSongChange();
                    }
                }
            }          

            if(currentSong == null)
            {
                currentSong = song;
                OnSongChange();
            }
        }
        #endregion
    }
}
