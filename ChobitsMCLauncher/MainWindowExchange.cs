using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChobitsMCLauncher
{
    public partial class MainWindow : Window
    {
        bool abort = false;
        public void InternetAbort()
        {
            Dispatcher.Invoke(() =>
            {
                abort = true;
                Background = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255));
                closeButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                closeButton.Foreground = new SolidColorBrush(Colors.White);
                reTryButtonGrid.Width = new GridLength(150, GridUnitType.Pixel);
                RetryButton.Tag = "Restart";
                mainGrid.Children.Remove(backGroundWeb);
                versionLable.Content = "网络异常";
                networkErrorGrid.Visibility = Visibility.Visible;
                processBar.IsIndeterminate = false;
                processBar.Value = 0.1;
                processBar.Foreground = new SolidColorBrush(Colors.Red);
                statusLable.Content = "网络错误";
                speedLable.Visibility = Visibility.Collapsed;
            });
        }
        public void FileAbort(string message)
        {
            Dispatcher.Invoke(() =>
            {
                reTryButtonGrid.Width = new GridLength(150, GridUnitType.Pixel);
                RetryButton.Tag = "ReTryUpdate";
                processBar.Foreground = new SolidColorBrush(Colors.Red);
                statusLable.Content = message;
            });
        }
        public void SetDisplayVersion(string version) => Dispatcher.Invoke(() => versionLable.Content = version);
        public static bool HasMainWindow()
        {
            if (instance == null) return false;
            else return true;
        }
        public void ChangeWindowWH(int width, int height)
        {
            Dispatcher.Invoke(() =>
            {
                int displayW = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                int displayH = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                Left = (displayW - width) / 2;
                Top = (displayH - height) / 2;
                Width = width;
                Height = height;
            });
        }
        public void UpdateStatus(string message, double process)
        {
            Dispatcher.Invoke(() =>
            {
                statusLable.Content = message;
                Thickness thickness = statusLable.Margin;
                thickness.Right = 10 + speedLable.ActualWidth;
                statusLable.Margin = thickness;
                if (process < 0 || process > 1) processBar.IsIndeterminate = true;
                else
                {
                    processBar.IsIndeterminate = false;
                    processBar.Value = process;
                }
            });
        }
    }
}
