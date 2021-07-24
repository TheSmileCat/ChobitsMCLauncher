using ChobitsMCLauncher.ProgramWindows;
using ChobitsMCLauncher.ProgramWindows.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using ChobitsMCLauncher.Tools.CheckUpdate;
using Panuon.UI.Silver;

namespace ChobitsMCLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static readonly int version = Version.version;
        /// <summary>
        /// 位于AppDatas目录下的本程序的data文件夹
        /// </summary>
        public static readonly string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Chobits Live\Minecraft\";
        /// <summary>
        /// 主机网络地址
        /// </summary>
        public static readonly string hostURL = "http://mc.porchwood.top:3080/";
        /// <summary>
        /// MC的网页地址
        /// </summary>
        public static readonly string minecraftURL = hostURL + "minecraft/";
        /// <summary>
        /// 本程序的检查更新网页地址
        /// </summary>
        public static readonly string updaterURL = minecraftURL + "updater/";
        /// <summary>
        /// 程序的启动地址
        /// </summary>
        public static readonly string startupPath = AppDomain.CurrentDomain.BaseDirectory;


        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            #region 出现问题的启动方式
            if(!File.Exists(dataFolder + @".install"))
            {
                //    try
                //    {
                if (MessageBox.Show("当前运行程序的位置将会是程序的安装目录，是否继续？", "欢迎使用ChobitsLive我的世界客户端", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) Environment.Exit(0);
                if(!Tools.ConsoleManager.IsAdministrator) Tools.ConsoleManager.RestartAsAdministrator(true);
                //        redo:
                //        MessageBox.Show("请选择一个安装目录，这将安装 ChobitsLive客户端应用程序 到指定目录。\n程序会自动在目录下创建安装目录");
                //        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                //        folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                //        folderBrowserDialog.Description = "选择安装目录";
                //        folderBrowserDialog.ShowDialog();
                //        if (folderBrowserDialog.SelectedPath == "") goto redo;
                //        string programPath = folderBrowserDialog.SelectedPath + @"\Chobits Live\Minecraft\";
                string programPath = AppDomain.CurrentDomain.BaseDirectory;
                //Directory.CreateDirectory(programPath);
                File.WriteAllText(dataFolder + ".install", programPath);
                Tools.ConsoleManager.CreateShortcutOnDesktop();
                if (!Environment.Is64BitOperatingSystem) MessageBox.Show("使用x86系统不适合玩MC，可能会卡顿，因为x86系统Java内存上限为1024MB");
                //        string targetFileName = programPath + new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Name;
                //        File.Copy(Process.GetCurrentProcess().MainModule.FileName, targetFileName);
                //        ProcessStartInfo processStartInfo = new ProcessStartInfo();
                //        processStartInfo.FileName = targetFileName;
                //        processStartInfo.Verb = "runas";
                //        Process.Start(processStartInfo);
                //        Environment.Exit(0);
                //    }
                //    catch(Exception ex)
                //    {
                //        MessageBox.Show(ex.StackTrace, ex.Message);
                //        Environment.Exit(-1);
                //    }
            }
            //Tools.CheckUpdate.ProgramEnvironmentCheck.CheckNuget();
            #endregion
            #region 检测是否有程序正在运行，正在运行就发送消息并退出
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
            #endregion
            //
            //版本检查
            BackgroundService backs2 = BackgroundService.Service;
            PreLaunch.CheckUpdater();
            ProgramEnvironmentCheck.CheckVersion();
            backs2.Show();
            backs2.Hide();
            //new BackgroundService().Show();
            new Thread(() =>
            {
                if (!Tools.CheckUpdate.PreLaunch.CheckGameEnvironment()) return;
                ClientMainWindow.UpdateStatusStatic("操作结束", 1);
                ClientMainWindow.UpdateButtonStatic(" 开始游戏", false);
                // TODO 临时操作，之后可能会更改
#if DEBUG
                if (ClientMainWindow.GetWindow() != null) ClientMainWindow.GetWindow().UpdateButton("开始游戏", false);
#else
                //Tools.GameLauncher.DoServer1ClientLaunch();
#endif
            }
            ).Start();
            await Task.Run(() => Thread.Sleep(1000));
            ClientMainWindow.GetWindow(true).Show();
            //Tools.ConsoleManager.Show();
            AppDomain.CurrentDomain.ProcessExit += (sender1, e1) =>
            {
                try { File.Delete(processID_Path); } catch { }
            };
        }

    }
}
