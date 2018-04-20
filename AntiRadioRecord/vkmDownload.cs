using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System.Net.Http;
using System.IO;

namespace AntiRadioRecord
{
    public class vkmDownload
    {
        #region Fields
        public string searchString { get; set; }
        private HttpClient client;
        private HtmlParser parser;
        private string adressForDownloadMusicVk = "https://downloadmusicvk.ru";
        private string mainAdressForDownloadMusicVk = "https://downloadmusicvk.ru/audio/search?q=";
        private string adressForMusic7s = "https://music7s.me/";

        private string mainAdressForMusic7s 
        {
            get
            {
                return "https://music7s.me/search.php?search=" + $"{searchString}";// + "&count=5&sort=2";
            }
        }
        public string downloadString;
        #endregion

        #region Constructores
        public vkmDownload(string searchString)
        {
            this.searchString = searchString;
            client = new HttpClient();          
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            parser = new HtmlParser();
        }

        public vkmDownload()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            parser = new HtmlParser();
        }
        #endregion

        #region Main Functions

        private async Task<string> getWebPageAsync(string HttpRrequest)
        {
            Stream data = await client.GetStreamAsync(HttpRrequest);
            StreamReader reader = new StreamReader(data, Encoding.GetEncoding("utf-8"));
            return reader.ReadToEnd();
        }

        #region Download from https://downloadmusicvk.ru

        private string getFirstLink(string webPage)
        {
            var doc = parser.Parse(webPage);
            string res = doc.All.Single(x => x.Id == "w1").Children[0].Children.Single(x => x.ClassName == "row audio vcenter").Children.Single(x=>x.ClassName== "col-lg-2 col-md-3 col-sm-4 col-xs-5").Children.Single(x=>x.ClassName== "btn btn-primary btn-xs download").GetAttribute("href");
            return res;
        }

        private string getSecondLink(string result)
        {
            var doc = parser.Parse(result);
            string res = doc.All.Single(x => x.Id == "download-audio").GetAttribute("href");
            downloadString = res;
            return res;
        }

        public async Task<byte[]> GetMp3FromDownloadMusicVkAsync()
        {
            string firstLink = getFirstLink((await getWebPageAsync(mainAdressForDownloadMusicVk + searchString)));
            string link = getSecondLink(await getWebPageAsync(adressForDownloadMusicVk + firstLink));
            byte[] file = await client.GetByteArrayAsync(new Uri(adressForDownloadMusicVk + link));
            return file;
        }

        public async Task<byte[]> GetMp3FromDownloadMusicVkAsync(string inputSearchString)
        {
            string firstLink = getFirstLink((await getWebPageAsync(mainAdressForDownloadMusicVk + inputSearchString)));
            string link = getSecondLink(await getWebPageAsync(adressForDownloadMusicVk + firstLink));
            byte[] file = await client.GetByteArrayAsync(new Uri(adressForDownloadMusicVk + link));
            return file;
        }
        #endregion

        #region Download from https://music7s.me/

        private async Task<string> GetDownloadlinkFromMusic7s(string inputSearchString)
        {
            searchString = inputSearchString;
            string webPage = await getWebPageAsync(mainAdressForMusic7s);
            var doc = parser.Parse(webPage);
            try
            {
                var element = doc.All.FirstOrDefault((x) => x.ClassName == "download_link ");
                if(element != null)
                {
                    return element.GetAttribute("href");
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<byte[]> GetMp3ForMusic7sAsync(string inputSearchString)
        {
            try
            {
                string downloadLink = await GetDownloadlinkFromMusic7s(inputSearchString.Replace(" ", "+").Replace("(", "%28").Replace(")", "%29"));
                byte[] file = await client.GetByteArrayAsync(new Uri(adressForMusic7s + downloadLink));
                return file;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #endregion

    }
}
