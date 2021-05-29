using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChobitsMCLauncher.ProgramWindows;

namespace ChobitsMCLauncher
{
    class Program
    {
        public static void Main()
        {
            Thread mainUIThread = new Thread(() =>
            {
                LauncherWindow.GetWindow().Show();
                new BackgroundService().Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            mainUIThread.SetApartmentState(ApartmentState.STA);
            mainUIThread.Start();
            while (!LauncherWindow.HasMainWindow())
            {
                Thread.Sleep(250);
            }
            new Thread(CheckUpdateThread.Run).Start();
        }
    }
}
