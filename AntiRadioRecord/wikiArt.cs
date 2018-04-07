using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace AntiRadioRecord
{

    class WikiArt
    {
        #region WikiArt Json Objects
        public class Artist
        {
            public string contentId { get; set; }
            public string artistName { get; set; }
            public string url { get; set; }
            public string lastNameFirst { get; set; }
            public string birthDay { get; set; }
            public string deathDay { get; set; }
            public string birthDayAsString { get; set; }
            public string deathDayAsString { get; set; }
            public string image { get; set; }
            public string wikipediaUrl { get; set; }
            public IList<string> dictonaries { get; set; }

            public Artist(string url)
            {
                this.url = url;
            }

            public Artist() { }
        }

        public class Painting
        {       
            public string title { get; set; }
            public string contentId { get; set; }
            public string artistContentId { get; set; }
            public string artistName { get; set; }
            public string completitionYear { get; set; }
            public string yearAsString { get; set; }
            public string width { get; set; }
            public string image { get; set; }
            public string height { get; set; }
            public string artistUrl { get; set; }
            public string url { get; set; }
            public IList<string> dictonaries { get; set; }
            public string location { get; set; }
            public string period { get; set; }
            public string serie { get; set; }
            public string genre { get; set; }
            public string material { get; set; }
            public string technique { get; set; }
            public string sizeX { get; set; }
            public string sizeY { get; set; }
            public string diameter { get; set; }
            public string auction { get; set; }
            public string yearOfTrade { get; set; }
            public string lastPrice { get; set; }
            public string galleryName { get; set; }
            public string tags { get; set; }
            public string description { get; set; }
        }
        #endregion

        #region Fields
        private string URL = "https://www.wikiart.org/en/App";

        HttpClient client;

        private int intraRequestCounter { get; set; }
        private int RequestCounter {
            get { return intraRequestCounter; }
            set
            {
                if (intraRequestCounter == 0 && value!=0)
                {
                    intraRequestCounter = value;
                    if(!timer.Enabled)
                    {
                        timer.Start();
                    }
                }
            }
        }
        public int requestCounter
        {
            get { return RequestCounter; }
        }

        private System.Timers.Timer timer;
        #endregion

        #region Constructors
        public WikiArt()
        {
            client = new HttpClient();
            RequestCounter = 0;
            timer = new System.Timers.Timer(5000);
            timer.Elapsed += (sender, e) =>
             {
                 if (RequestCounter != 0)
                 {
                     RequestCounter = 0;
                 }
             };
        }
        #endregion

        #region Gets Of Objects
        public async Task<List<Painting>> GetArtistPaintings(Artist artist)
        {
            if(RequestCounter < 10)
            {
                try
                {
                    string responseFromServer = "";
                    using (Stream stream = await client.GetStreamAsync(URL + "/Painting/PaintingsByArtist?artistUrl=" + artist.url.Replace(" ", "") + "&json=2"))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                    return JsonConvert.DeserializeObject<List<Painting>>(responseFromServer);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
        }

        public async Task<List<Painting>> GetArtistPaintings(string artistURL)
        {
            if (RequestCounter < 10)
            {
                try
                {
                    string responseFromServer = "";
                    using (Stream stream = await client.GetStreamAsync(URL + "/Painting/PaintingsByArtist?artistUrl=" + artistURL.Replace(" ", "") + "&json=2"))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                    return JsonConvert.DeserializeObject<List<Painting>>(responseFromServer);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
        }

        public async Task<List<Artist>> GetAllArtists()
        {
            if (RequestCounter < 10)
            {
                try
                {
                    string responseFromServer = "";
                    using (Stream stream = await client.GetStreamAsync(URL + "/Artist/AlphabetJson"))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                    return JsonConvert.DeserializeObject<List<Artist>>(responseFromServer);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
        }

        public async Task<string> GetRandomArtistPainting(Artist artist)
        {
            Random random = new Random((int)DateTime.Now.ToBinary());
            var paintgs = await GetArtistPaintings(artist);
            return paintgs[random.Next(0, paintgs.Count - 1)].image.Split('!')[0];
        }

        public async Task<byte[]> GetPaintingImage(Painting painting)
        {
            if(RequestCounter < 10)
            {
                try
                {
                    return await client.GetByteArrayAsync(painting.image.Split('!')[0]);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    RequestCounter++;
                }
            }
            else
            {
                throw new Exception("errorCode:1 |The limitation of requests was reached|");
            }
            
        }
        #endregion

        #region Writers Into File
        public void WriteArtistsIntoFile(List<Artist> Artists,string path)
        { 
            foreach (var artist in Artists)
            {
                File.AppendAllText(path, JsonConvert.SerializeObject(artist));
            }
        }

        public void WritePaintingsIntoFile(List<Painting> Paintings, string path)
        {
            foreach (var painting in Paintings)
            {
                File.AppendAllText(path, JsonConvert.SerializeObject(painting));
            }
        }

        public async Task WriteImageIntoFile(byte[] image, string path)
        {
            using (var writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {
                await Task.Run(() =>
                {
                    writer.Write(image);
                });
            }
        }
        #endregion

        #region Readers From File
        public async Task<List<Artist>> ReadArtistsFromFile(string path)
        {
            Task<List<Artist>> task = new Task<List<Artist>>(() =>
            {
                return JsonConvert.DeserializeObject<List<Artist>>(File.ReadAllText(path));
            });
            task.Start();
            return await task;
        }

        public async Task<List<Painting>> ReadPaintingsFromFile(string path)
        {
            Task<List<Painting>> task = new Task<List<Painting>>(() =>
            {
                return JsonConvert.DeserializeObject<List<Painting>>(File.ReadAllText(path));
            });
            task.Start();
            return await task;
        }
        #endregion

        #region Examples of Usage
        public async Task<string> CreateWikiArtDataDump()
        {
            var allArtists = await GetAllArtists();
            WriteArtistsIntoFile(allArtists, Directory.GetCurrentDirectory() + "\\artists.json");
            for (int i = 0; i < allArtists.Count;)
            {
                if (RequestCounter < 10)
                {
                    try
                    {
                        string pathToDir = Directory.GetCurrentDirectory() + "\\" + allArtists[i].artistName.Trim();
                        if (!Directory.Exists(pathToDir))
                        {
                            DirectoryInfo directoryInfo = Directory.CreateDirectory(pathToDir);
                        }
                        var artistPaintings = await GetArtistPaintings(allArtists[i]);
                        string pathToFile = Directory.GetCurrentDirectory() + "\\" + allArtists[i].artistName.Trim() + "\\" + allArtists[i].artistName.Trim() + ".json";
                        if(!File.Exists(pathToFile))
                        {
                            WritePaintingsIntoFile(artistPaintings, pathToFile);
                        }
                        i++;
                    }
                    catch
                    {
                        i++;
                    }
                }
            }
            return "done";
        }

        public async Task SaveAllArtistPaintingsImages(Artist artist)
        {
            var artistPaintings = await GetArtistPaintings(artist);
            string path = Directory.GetCurrentDirectory() + "\\" + artist.artistName.Trim(); 
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            }
            for (int i = 0; i < artistPaintings.Count;)
            {
                if (RequestCounter < 10)
                {
                    try
                    {
                        string pathToFile = Directory.GetCurrentDirectory() + "\\" + artist.artistName.Trim() + "\\" + artistPaintings[i].title.Trim() + ".jpg";
                        if(!File.Exists(pathToFile))
                        {
                            await WriteImageIntoFile(await GetPaintingImage(artistPaintings[i]), Directory.GetCurrentDirectory() + "\\" + artist.artistName.Trim() + "\\" + artistPaintings[i].title.Trim() + ".jpg");
                        }
                        i++;
                    }
                    catch (Exception ex)
                    {
                        i++;
                        throw ex;
                    }
                }                
            }
        }
        #endregion
    }
}
