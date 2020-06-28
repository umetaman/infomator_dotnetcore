using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace Infomator
{
    public static class InfomatorHelper
    {
        public static Stream GetGET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            var response = request.GetResponse();
            return response.GetResponseStream();
        }

        public static string StreamToText(Stream stream)
        {
            using(var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Encoding.UTF8.GetString(ms.GetBuffer());
            }
        }
    }
}
