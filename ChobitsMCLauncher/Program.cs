using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChobitsMCLauncher
{
    class Program
    {
        public static void Main()
        {
            Thread mainwindow = new Thread(() =>
            {
                MainWindow.GetMainWindow().Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            mainwindow.SetApartmentState(ApartmentState.STA);
            mainwindow.Start();
            while (!MainWindow.HasMainWindow())
            {
                Thread.Sleep(500);
            }
            new Thread(CheckUpdateThread.Run).Start();
        }
    }
}
