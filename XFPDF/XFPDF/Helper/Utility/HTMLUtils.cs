using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace XFPDF
{
    public static class HTMLUtils 
    {
        public async static Task<string> GetHTMLFromURL(string url)
        {
            //url = "https://aka.ms/xamarinforms-previewer";
            string htmlstring = string.Empty;

            //Used to get the html string from url if url does not have images.
            //using (WebClient client = new WebClient())
            //{
            //    htmlstring = client.DownloadString(url);
            //}

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //StreamReader sr = new StreamReader(response.GetResponseStream());
            //htmlstring = sr.ReadToEnd();
            //sr.Close();

            var client = new HttpClient();
            htmlstring = await client.GetStringAsync(url);

            return htmlstring;
        }
    }
}
