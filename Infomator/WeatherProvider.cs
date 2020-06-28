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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Windows.Navigation;

    class WeatherProvider
    {
        public class Weather
        {
            public string Location { private set; get; }
            public string Main { private set; get; }
            public string Description { private set; get; }
            public int TemperatureMax { private set; get; }
            public int TemperatureMin { private set; get; }

            public static Weather FromJson(string json)
            {
                JObject root = JObject.Parse(json);

                return new Weather()
                {
                    Location = (string)root["name"],
                    Main = (string)root["weather"][0]["main"],
                    Description = (string)root["weather"][0]["description"],
                    TemperatureMax = (int)root["main"]["temp_max"],
                    TemperatureMin = (int)root["main"]["temp_min"]
                };
            }

            public override string ToString()
            {
                return string.Join("\n", new string[]
                {
                    string.Format("Location: {0}", Location),
                    string.Format("Main: {0}", Main),
                    string.Format("Description: {0}", Description),
                    string.Format("TemperatureMax: {0}", TemperatureMax),
                    string.Format("TemperatureMin: {0}", TemperatureMin),
                });
            }
        }

        private static readonly string _BaseUrl = "http://api.openweathermap.org/data/2.5/weather?units=metric&{0}";
        private static readonly string _Query = "q={0}&APPID={1}";

        public void GetWeather(Action<Weather> onLoaded)
        {
            var key = Settings.Settings.UserWeatherAPI.AccessKey;
            var location = Settings.Settings.UserWeatherAPI.Location;
            Task task = Task.Run(() => GetWeatherThread(key, location, onLoaded));
        }

        // Taskに渡す用
        private void GetWeatherThread(string key, string location, Action<Weather> onLoaded)
        {
            string url = string.Format(_BaseUrl, string.Format(_Query, location, key));
            Stream responseStream = InfomatorHelper.GetGET(url);

            var text = InfomatorHelper.StreamToText(responseStream);
            responseStream.Close();

            var weather = Weather.FromJson(text);
            onLoaded(weather);
        }
    }
}
