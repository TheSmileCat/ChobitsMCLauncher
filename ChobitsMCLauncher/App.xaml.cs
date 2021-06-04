using ChobitsMCLauncher.ProgramWindows;
using ChobitsMCLauncher.ProgramWindows.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChobitsMCLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string dataFolder { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Chobits Live\Minecraft\";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            string processID_Path = Path.Combine(dataFolder, ".processid");
            try
            {
                if (File.Exists(processID_Path))
                {
                    try
                    {
                        Process process = Process.GetProcessById(int.Parse(File.ReadAllText(processID_Path)));
                        IntPtr backs = Tools.ConsoleManager.FindWindow(null, "ChobitsLive Minecraft Client Background Service");
                        Tools.ConsoleManager.PostMessage(backs, 0xffff, 0, 0);
                        //MessageBox.Show(Tools.ConsoleManager.GetWindowTitle(backs),"发送数据");
                        Environment.Exit(0);
                        return;
                    }
                    catch (Exception ex){
                        //MessageBox.Show(ex.StackTrace, ex.Message);
                    }
                }
                FileStream fs = File.Create(processID_Path);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(Process.GetCurrentProcess().Id);
                sw.Dispose();
                fs.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.StackTrace, ex.Message);
            }
            //MessageBox.Show("正常启动");
            BackgroundService backs2 = new BackgroundService();
            backs2.Show();
            backs2.Hide();
            //new BackgroundService().Show();
            ClientMainWindow.GetWindow().Show();
            //Tools.ConsoleManager.Show();
            AppDomain.CurrentDomain.ProcessExit += (sender1, e1) =>
            {
                try { File.Delete(processID_Path); } catch { }
            };
        }
    }
}
