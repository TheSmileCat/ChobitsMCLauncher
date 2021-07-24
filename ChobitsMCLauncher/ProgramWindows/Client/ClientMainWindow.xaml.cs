using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Timer = System.Windows.Forms.Timer;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;

namespace ChobitsMCLauncher.ProgramWindows.Client
{
    /// <summary>
    /// ClientMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientMainWindow : Window
    {
        private static bool firstShow = true;
        private static ClientMainWindow instance = null;
        private static int tabWindowSelectIndex = 0;
        private static int normalServer = 0;
        private bool WindowMoving = false;
        private ClientMainWindow()
        {
            InitializeComponent();
        }
        public static ClientMainWindow GetWindow(bool canCreate=false) {
            if (canCreate)
            {
                return instance ?? (instance = new ClientMainWindow());
            }
            return instance; 
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
            firstShow = false;
            //if(ButtonHelper.GetIsWaiting(LaunchButton)) Notice.Show("ChobitsMC服务正在后台运行\n操作完成后会自动通知", "提示", 3, MessageBoxIcon.Info);
        }

        private void TabItemButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            int index = int.Parse(button.Tag as string);
            ServerPage.SelectedIndex = index;
            isNormalServer.IsChecked = normalServer == index;
        }


        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border button = sender as Border;
            switch (button.Name)
            {
                case "EnMinily":
                    Dispatcher.BeginInvoke((Action)delegate() {
                        WindowState = WindowState.Minimized;
                    });
                    break;
                case "Change":
                    if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
                    else WindowState = WindowState.Maximized;
                    break;
                case "Exit":
                    Close();
                    break;
            }
        }

        #region 无边框拖动效果
        [DllImport("user32.dll")]//拖动无窗体的控件
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        public void Moving(object sender, MouseEventArgs e)
        {
            //拖动窗体
            ReleaseCapture();
            SendMessage(new WindowInteropHelper(this).Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
            WindowMoving = true;
        }
        private void WindowTitle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WindowMoving = false;
        }
        #endregion

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SyncTitle.Content = "ChobitsLive Minecraft Official Server - Control Window - Version " + App.version;
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
            ServerPage.SelectedIndex = tabWindowSelectIndex;
            TabItemChangeButton.IsEnabled = !isWaiting;
            ServerPage.SelectedIndex = normalServer >= 0 ? normalServer : 0;
            isNormalServer.IsChecked = normalServer == 0;
            if (isWaiting) UpdateButton("操作进行中", true);
#if !DEBUG
            //if (firstShow && Tools.CheckUpdate.PreLaunch.IsEnvironmentCheakPassed)
            //{
            //    Task<bool> task = new Task<bool>(Tools.GameLauncher.DoServer1ClientLaunch);
            //    task.Start();
            //    bool status = await task;
            //    if (status)
            //    {
            //        await Task.Run(() =>
            //        {
            //            Thread.Sleep(3000);
            //            UpdateButton("开始游戏", false);
            //            Dispatcher.Invoke(Close);
            //        });
            //    }
            //}
#endif
        }

        private async void LaunchButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!Tools.CheckUpdate.PreLaunch.IsEnvironmentCheakPassed)
            {
                MessageBox.Show("你需要等待游戏运行环境检查完成");
                return;
            }
            bool status = true;
            switch (ServerPage.SelectedIndex)
            {
                case 0:
                    status = await Task.Run(Tools.GameLauncher.DoServer1ClientLaunch);
                    break;
                case 1:
                    status = await Task.Run(Tools.GameLauncher.DoServer3ClientLaunch);
                    break;
            }
            if (!status) MessageBox.Show("启动失败了……", "错误");
            UpdateButton(" 开始游戏", false);
            UpdateStatus("", 1);
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag)
            {
                case "ACupOfJava":
                    {
                        try
                        {
                            Process.Start("NVIDIAControlPanel");
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.StackTrace, ex.Message);
                        }
                    }
                    break;
                case "Setting":
                    Setting.GetWindow().ShowDialog(this);
                    break;
            }
        }

        private async void isNormalServer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int thisServer = ServerPage.SelectedIndex;
            CheckBox checkBox = sender as CheckBox;
            await Task.Run(() => Thread.Sleep(200));
            if (checkBox.IsChecked == true)
            {
                normalServer = thisServer;
            }
            else if (thisServer == normalServer) normalServer = -1;
        }
    }
}
