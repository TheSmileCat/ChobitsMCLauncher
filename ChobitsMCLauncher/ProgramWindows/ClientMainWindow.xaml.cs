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

namespace ChobitsMCLauncher.ProgramWindows
{
    /// <summary>
    /// ClientMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientMainWindow : Window
    {

        private static ClientMainWindow instance = null;
        private ClientMainWindow()
        {
            InitializeComponent();
        }
        public static ClientMainWindow GetWindow() { return instance == null ? (instance = new ClientMainWindow()) : instance; }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
        }
    }
}
