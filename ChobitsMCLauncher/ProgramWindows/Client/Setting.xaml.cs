using ChobitsMCLauncher.SQL;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChobitsMCLauncher.ProgramWindows.Client
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : WindowX
    {
        public static Setting Window { get; private set; }
        public static DataBase DataBase { get; private set; } = new DataBase();
        public static Setting GetWindow()
        {
            return Window ?? (Window = new Setting());
        }
        private Setting()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {

        }

        private void WindowX_Closed(object sender, EventArgs e)
        {
            Window = null;
        }
        public void ShowDialog(Window window)
        {
            Owner = window;
            ShowDialog();
        }
    }
}
