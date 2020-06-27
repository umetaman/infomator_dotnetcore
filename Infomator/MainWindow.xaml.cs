using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Infomator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ウィンドウの位置を固定
            FixWindowPosition();

            SetWeather("Sendai, Miyagi", "20℃, 50%");
            SetNewsContent("週末は九州で大雨に厳重警戒　道路冠水や河川増水のおそれも");
        }

        public static void OpenUrl(string url)
        {
            // https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
            try
            {
                System.Diagnostics.Process.Start("url");
            }
            catch
            {
                // Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
            }
        }

        public void SetWeather(string location, string content)
        {
            WeatherLocationText.Text = location;
            WeatherContentText.Text = content;
        }

        public void SetNewsContent(string content)
        {
            NewsContentText.Text = content;
        }

        #region private callback
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            StartAnimation();
        }

        private void OnClickNewsContent(object sender, MouseButtonEventArgs e)
        {
            var newsProvider = new NewsProvider();
        }
        #endregion

        #region private methods
        private void StartAnimation(EventHandler onComplete = null)
        {
            // 横幅いっぱいを通過するのにかける時間
            int secondsPerWidth = 10;
            int repeatCount = 2;
            int duration = (int)Math.Round(NewsContentText.ActualWidth * 2 / this.Width) * secondsPerWidth;

            // 横幅当たりの経過時間を計算する
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = NewsCanvas.ActualWidth;
            animation.To = NewsContentText.ActualWidth * -1;
            animation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            animation.RepeatBehavior = new RepeatBehavior(repeatCount);

            if(onComplete != null)
            {
                animation.Completed += onComplete;
            }

            NewsContentText.BeginAnimation(Canvas.LeftProperty, animation);
        }

        private void FixWindowPosition()
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
        }

        #endregion
    }
}
