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
        public BackgroundService()
        {
            InitializeComponent();
            Init();
        }
        bool toClose = false;
        private void BackgroundService_Load(object sender, EventArgs e)
        {
            Hide();
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
        }

        private void ProgramMainIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    ClientMainWindow window = ClientMainWindow.GetWindow();
                    window.Show();
                    break;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientMainWindow window = ClientMainWindow.GetWindow();
            window.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            programMainIcon.Visible = false;
            Environment.Exit(0);
        }
    }
}
