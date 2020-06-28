using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infomator.Settings
{
    public class WeatherAPI
    {
        // APIキーは絶対にデフォルトで指定しちゃだめ。ユーザーに取得してもらう。
        public static WeatherAPI Default { get => new WeatherAPI(string.Empty, "Sendai, JP"); }

        public string AccessKey { private set; get; }
        public string Location { private set; get; }

        public WeatherAPI(string accessKey, string location)
        {
            AccessKey = accessKey;
            Location = location;
        }

        public override string ToString()
        {
            return string.Join("\n", new string[]
            {
                string.Format("Weather_AccessKey: {0}", AccessKey),
                string.Format("Weather_Location : {0}", Location)
            });
        }
    }

    public class GoogleNewsAPI
    {
        public static GoogleNewsAPI Default
        {
            get => new GoogleNewsAPI(NewsTopic.TECHNOLOGY);
        }

        public NewsTopic Topic { private set; get; }

        public GoogleNewsAPI(NewsTopic topic)
        {
            Topic = topic;
        }

        public override string ToString()
        {
            return string.Join("\n", new string[]
            {
                "News_Topic: " + Topic.ToString()
            });
        }
    }

    public static class Settings
    {
        // 設定のバージョン
        public static readonly string Version = "1";
        public static bool IsLoaded { get => _Data != null; }
        public static string UserVersion { private set; get; }
        public static GoogleNewsAPI UserNewsAPI { private set; get; }
        public static WeatherAPI UserWeatherAPI { private set; get; }

        // 読み込んだデータ
        private static JObject _Data = null;
        // 設定ファイル
        private static readonly string _VersionDataKey = "Version";
        private static readonly string _NewsAPIDataKey = "GoogleNewsAPI";
        private static readonly string _WeatherAPIDataKey = "WeatherAPI";
        private static readonly string _DefaultFileName = "settings.json";
        private static readonly string _DefaultPath = Directory.GetCurrentDirectory();
        private static string _DefaultSettingFilePath { get => Path.Combine(_DefaultPath, _DefaultFileName); }

        public static void Read()
        {
            Read(_DefaultSettingFilePath);
        }

        public static void Read(string filePath)
        {
            using (StreamReader reader = File.OpenText(filePath))
            using (JsonTextReader textReader = new JsonTextReader(reader))
            {
                _Data = (JObject)JToken.ReadFrom(textReader);
            }

            if(_Data.HasValues == false)
            {
                throw new Exception("Read Json Data does not have values.");
            }
#if DEBUG
            var requiredKeys = new string[]
            {
                _VersionDataKey, _NewsAPIDataKey, _WeatherAPIDataKey
            };

            if(requiredKeys.Any(key => _Data.ContainsKey(key) == false))
            {
                Console.WriteLine("Read Json Data does not have required keys or values.");      
            }
#endif

            // 読み込んで、キーがなかったらデフォルトの値を読む
            UserVersion = _Data.ContainsKey(_VersionDataKey) ? _Data.Value<string>(_VersionDataKey) : Version;
            UserNewsAPI = _Data.ContainsKey(_NewsAPIDataKey) ? _Data.GetValue(_NewsAPIDataKey).ToObject<GoogleNewsAPI>() : GoogleNewsAPI.Default;
            UserWeatherAPI = _Data.ContainsKey(_WeatherAPIDataKey) ? _Data.GetValue(_WeatherAPIDataKey).ToObject<WeatherAPI>() : WeatherAPI.Default;
#if DEBUG
            Console.WriteLine("[Loaded User Settings]");
            Console.WriteLine("Version: " + UserVersion);
            Console.WriteLine(UserNewsAPI.ToString());
            Console.WriteLine(UserWeatherAPI.ToString());
#endif
        }

        public static void Write(string filePath, JObject data)
        {
            using(StreamWriter writer = File.CreateText(filePath))
            using(JsonTextWriter textWriter = new JsonTextWriter(writer))
            {
                data.WriteTo(textWriter);
            }
        }

        public static void CreateDefaultSettings()
        {
#if !DEBUG
            if (File.Exists(_DefaultSettingFilePath))
            {
                // すでに存在しているときは作成しない
                return;
            }
#endif

            JObject defaultJsonData = new JObject(
                new JProperty(_VersionDataKey, Version),
                new JProperty(_NewsAPIDataKey, JObject.FromObject(GoogleNewsAPI.Default)),
                new JProperty(_WeatherAPIDataKey, JObject.FromObject(WeatherAPI.Default))
                );

            Write(_DefaultSettingFilePath, defaultJsonData);
        }
    }
}
