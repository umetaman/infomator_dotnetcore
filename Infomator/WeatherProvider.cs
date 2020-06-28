using System;
using System.Collections.Generic;
using System.Text;

namespace Infomator
{
    using Infomator.Settings;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    class WeatherProvider
    {
        private static readonly string _BaseUrl = "http://api.openweathermap.org/data/2.5/weather?units=metric&{0}";
        private static readonly string _Query = "q={0}&APPID={1}";

        public void GetWeather(Action onLoaded)
        {
            var key = Settings.Settings.UserWeatherAPI.AccessKey;
            var location = Settings.Settings.UserWeatherAPI.Location;
            Task task = Task.Run(() => GetWeatherThread(key, location, () => { }));
        }

        // Taskに渡す用
        private void GetWeatherThread(string key, string location, Action onLoaded)
        {
            string url = string.Format(_BaseUrl, string.Format(_Query, location, key));
            Stream responseStream = InfomatorHelper.GetGET(url);

            var text = InfomatorHelper.StreamToText(responseStream);
            responseStream.Close();
            Console.WriteLine(text);
        }
    }
}
