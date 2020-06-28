using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.CodeDom;

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

            public override string ToString()
            {
                return string.Join("\n", new string[]
                {
                    Title, Url
                });
            }
        }

        private static readonly string _TopicBaseUrl = "https://news.google.com/news/rss/headlines/section/topic/{0}?{1}";
        private static readonly string _LanguageQuery = "hl=ja&gl=JP&ceid=JP:ja";
        
        private List<Headline> _Headlines = new List<Headline>();
        private int _Index = 0;
        private Headline _CurrentHeadline = null;

        public Headline CurrentHeadline { get => _CurrentHeadline; }

        public void GetRSSDocument(NewsTopic topic, Action onLoaded)
        {
            // 別スレッドで済ます
            Task task = Task.Run(() =>
            {
                string url = string.Format(_TopicBaseUrl, topic.ToString(), _LanguageQuery);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                var response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                // XMLが返ってくるのでパースする
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(responseStream);

                var headlines = ParseXmlDocumentToHeadLines(xmlDocument);
                _Headlines.Clear();
                _Headlines = headlines.ToList();
                _Index = 0;

                onLoaded();
            });
        }

        // 実行したときに表示するHeadlineを更新する。
        // nullによって終了を示す
        public Headline GetNext()
        {
            Headline headline = null;

            if(_Index < _Headlines.Count)
            {
                headline = _Headlines[_Index];
                _Index++;
            }

            // 外部から最小したいので保存する
            _CurrentHeadline = headline;

            return headline;
        }

        private IEnumerable<Headline> ParseXmlDocumentToHeadLines(XmlDocument document)
        {
            //root:<rss> -> <channel> -> <item>とたどる
            // 先頭は必ずRSSだと決まっている
            var documentElement = document.DocumentElement;
            if(documentElement.FirstChild.Name == "channel")
            {
                var channelNode = documentElement.FirstChild;
                var channelEnumerator = channelNode.ChildNodes.GetEnumerator();
                var headlines = new List<Headline>();

                while (channelEnumerator.MoveNext())
                {
                    var item = channelEnumerator.Current as XmlNode;
                    if(item.Name == "item")
                    {
                        var title = FindFirstNode(item.ChildNodes, "title").InnerText;
                        var link = FindFirstNode(item.ChildNodes, "link").InnerText;

                        headlines.Add(new Headline { Title = title, Url = link });
                    }
                }

                return headlines;
            }
            else
            {
                throw new Exception("XML Parse Error. Value of XmlDocument must not be null.");
            }

            return null;
        }

        private XmlNode FindFirstNode(XmlNodeList nodeList, string targetName)
        {
            var enumerator = nodeList.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    XmlNode node = enumerator.Current as XmlNode;
                    if(node.Name == targetName)
                    {
                        return node;
                    }
                }

                // ここまで来たらエラー
                throw new Exception("Parsing XML Error.");
            }
            catch(Exception e)
            {
                // :(
            }

            return null;
        }
    }
}
