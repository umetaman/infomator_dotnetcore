using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infomator
{
    public enum NewsTopic
    {
        BUSINESS,
        TECHNOLOGY,
        ENTERTAINMENT,
        SPORTS
    }

    public class NewsProvider
    {
        public class Headline
        {
            public string Title = string.Empty;
            public string Url = string.Empty;
        }

        private static readonly string TopicBaseUrl = "https://news.google.com/news/rss/headlines/section/topic/{0}?{1}";
        private static readonly string LanguageQuery = "hl=ja&gl=JP&ceid=JP:ja";

        public void GetRSSDocument(Action<string> onComplete)
        {
            Task<string> task = Task.Run(() =>
            {
                string url = string.Format(TopicBaseUrl, NewsTopic.TECHNOLOGY.ToString(), LanguageQuery);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                var response = request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string document = streamReader.ReadToEnd();

                streamReader.Close();
                response.Close();

                onComplete(document);
                return document;
            });
        }
    }
}
