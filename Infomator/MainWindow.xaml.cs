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
using System.Windows.Threading;
using Infomator.Settings;

namespace Infomator
{
    using Settings;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NewsProvider _NewsProvider;

        public MainWindow()
        {
            InitializeComponent();

            // ウィンドウの位置を固定
            FixWindowPosition();

            // テキストを初期化
            SetWeather(string.Empty, string.Empty);
            SetNewsContent(string.Empty);

            // ニュースを取得するオブジェクト
            _NewsProvider = new NewsProvider();

            // 設定ファイルの準備
            Settings.Settings.CreateDefaultSettings();
            Settings.Settings.Read();
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
            UpdateHeadlines();
        }

        private void OnClickNewsContent(object sender, MouseButtonEventArgs e)
        {
            var currentHeadline = _NewsProvider.CurrentHeadline;
            if(currentHeadline != null)
            {
                OpenUrl(currentHeadline.Url);
            }
        }
        #endregion

        #region private methods
        private void UpdateHeadlines()
        {
            // ニュースを取得して、Loopを開始する
            _NewsProvider.GetRSSDocument(NewsTopic.TECHNOLOGY, () => SetNextHeadline());
        }

        private void SetNextHeadline()
        {
            var currentHeadline = _NewsProvider.GetNext();
            if(currentHeadline != null)
            {
                var title = currentHeadline.Title;
                var link = currentHeadline.Url;

                Console.WriteLine(currentHeadline.ToString());

                Dispatcher.Invoke(() =>
                {
                    SetNewsContent(title);
                    DoEvents();
                });
                Dispatcher.Invoke(() => StartAnimation((sender, e) => SetNextHeadline()));
            }
            else
            {
                Console.WriteLine("SetNextHeadline() > UpdateHeadlines()");
                // nullなら再取得
                UpdateHeadlines();
            }
        }

        private void StartAnimation(EventHandler onComplete = null)
        {
            // UIのレイアウト更新
            NewsCanvas.UpdateLayout();
            NewsContentText.UpdateLayout();

            // 横幅いっぱいを通過するのにかける時間
            int secondsPerWidth = 3;
            int repeatCount = 2;
            int duration = (int)Math.Round(NewsContentText.ActualWidth * 2 / this.Width) * secondsPerWidth;

            Console.WriteLine(string.Format("TextWidth: {0}, CanvasWidth: {1}", NewsCanvas.ActualWidth, NewsContentText.ActualWidth));
            Console.WriteLine("/Width: " + (int)Math.Round(NewsContentText.ActualWidth * 2 / this.Width));
            Console.WriteLine("ScrollDuration: " + duration);

            // 横幅当たりの経過時間を計算する
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = NewsCanvas.ActualWidth;
            animation.To = NewsContentText.ActualWidth * -1;
            animation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            animation.RepeatBehavior = new RepeatBehavior(repeatCount);

            if (onComplete != null)
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

        // 再描画
        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}
