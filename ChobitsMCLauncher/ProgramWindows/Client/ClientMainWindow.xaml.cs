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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChobitsMCLauncher.ProgramWindows.Client
{
    /// <summary>
    /// ClientMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientMainWindow : Window
    {

        private static ClientMainWindow instance = null;
        private bool WindowMoving = false;
        private ClientMainWindow()
        {
            InitializeComponent();
        }
        public static ClientMainWindow GetWindow() { return instance ?? (instance = new ClientMainWindow()); }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
        }

        private void TabItemButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            int index = int.Parse(button.Tag as string);
            ServerPage.SelectedIndex = index;
        }

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border button = sender as Border;
            switch (button.Name)
            {
                case "EnMinily":
                    WindowState = WindowState.Minimized;
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
    }
}
