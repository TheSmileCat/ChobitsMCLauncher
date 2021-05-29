using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Timer = System.Windows.Forms.Timer;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChobitsMCLauncher.ProgramWindows
{
    /// <summary>
    /// LauncherWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LauncherWindow : Window
    {
        private static LauncherWindow instance = null;
        private LauncherWindow()
        {
            InitializeComponent();
        }
        public static LauncherWindow GetWindow() { return instance == null ? (instance = new LauncherWindow()) : instance; }


        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag)
            {
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag)
            {
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Button button = sender as Button;
            switch (button.Tag)
            {
                case "Restart":
                    instance = null;
                    Program.Main();
                    Close();
                    break;
            }
        }
        private delegate void VoidDelegate();

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            switch (label.Tag)
            {
                case "CloseButton":
                    if (abort) return;
                    label.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    label.Foreground = new SolidColorBrush(Colors.White);
                    break;
            }
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            switch (label.Tag)
            {
                case "CloseButton":
                    if (abort) return;
                    label.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                    label.Foreground = new SolidColorBrush(Colors.Transparent);
                    break;
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            switch (label.Tag)
            {
                case "CloseButton":
                    if (abort) Environment.Exit(0);
                    else if (MessageBox.Show("你确定要退出吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        Hide();
                        Dispatcher.BeginInvoke(new VoidDelegate(() => Environment.Exit(0)));
                    }
                    break;
            }
        }
        private bool isLoaded = false;
        public bool GetIsLoaded() { return isLoaded; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            Timer internetSpeedTimer = new Timer();
            internetSpeedTimer.Interval = 1000;
            ulong lastIS = Tools.HTTP.GetDataStatistics();
            internetSpeedTimer.Tick += (object sender1, EventArgs e1) =>
            {
                ulong nowIS = Tools.HTTP.GetDataStatistics();
                ulong add = nowIS - lastIS;
                lastIS = nowIS;
                if (add < 1024) speedLable.Content = add + " b/s";
                else if (add < 1048576) speedLable.Content = (double)(add * 10 / 1024) / 10 + " kb/s";          //1024^2=1048576
                else if (add < 1073741824) speedLable.Content = (double)(add * 10 / 1048576) / 10 + " Mb/s";    //1024^3=1073741824
                else speedLable.Content = (double)(add * 10 / 1073741824) / 10 + " Gb/s";
                //speedLable.Content = "" + nowIS;
            };
            internetSpeedTimer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
        }
    }
}
