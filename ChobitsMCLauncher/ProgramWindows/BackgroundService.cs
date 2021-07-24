using ChobitsMCLauncher.ProgramWindows.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChobitsMCLauncher.ProgramWindows
{
    public partial class BackgroundService : Form
    {
        private System.Windows.Threading.Dispatcher Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        public static BackgroundService Service { get; private set; } = new BackgroundService();
        public static BackgroundService GetService()
        {
            return Service ?? (Service = new BackgroundService());
        }
        private BackgroundService()
        {
            InitializeComponent();
            Init();
        }
        bool toClose = false;
        private void BackgroundService_Load(object sender, EventArgs e)
        {
            //Hide();
        }
        private void BackgroundService_Closing(object sender, FormClosingEventArgs e)
        {
            if (!toClose)
            {
                e.Cancel = true;
                Hide();
            }
        }
        public void CloseIt()
        {
            Dispatcher.Invoke(() =>
            {
                toClose = true;
                Close();
            });
        }
        private void Init()
        {
            programMainIcon.MouseClick += ProgramMainIcon_MouseClick;
            //programMainIcon.ShowBalloonTip(0, "ChobitsMC综合服务", "应用程序正在启动，请稍候...", ToolTipIcon.Info);
        }
        /// <summary>
        /// 显示一个
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        public static void ShowBalloonMessage(string message, string title = "ChobitsMC综合服务", ToolTipIcon icon = ToolTipIcon.Info)
        {
            Service.ShowMessage(title, message, icon);
        }

        public void ShowMessage(string message, string title, ToolTipIcon icon)
        {
             System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(()=>
             {
                 programMainIcon.ShowBalloonTip(0, message, title, icon);
             });
        }

        private void ProgramMainIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ClientMainWindow window = ClientMainWindow.GetWindow(true);
                    window.Show();
                    break;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientMainWindow window = ClientMainWindow.GetWindow(true);
            window.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            programMainIcon.Visible = false;
            Environment.Exit(0);
        }
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0xffff:
                    ClientMainWindow.GetWindow(true).Show();
                    ClientMainWindow.GetWindow(true).Activate();
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void programMainIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting.GetWindow().Show();
        }
    }
}
