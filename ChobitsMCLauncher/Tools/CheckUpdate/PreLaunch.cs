using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ChobitsMCLauncher.ProgramWindows.Client;
namespace ChobitsMCLauncher.Tools.CheckUpdate
{
    class PreLaunch
    {
        static string local_path = AppDomain.CurrentDomain.BaseDirectory;
        static string java_path_builder = local_path + @"Java\Java{0}\";
        public static bool IsEnvironmentCheakPassed { get; private set; } = false;
        public static bool CheckGameEnvironment()
        {
            
            if (!(CheckJava() && CheckJavaFX())) MessageBox.Show("检查运行环境失败...");
            else return IsEnvironmentCheakPassed = true;
            return false;
        }
        /// <summary>
        /// 默认的Java检查，用于启动HMCL
        /// </summary>
        /// <returns>是否检查成功</returns>
        public static bool CheckJava()
        {
            return CheckJava(16);
        }
        /// <summary>
        /// 通用的Java检查，用于启动游戏
        /// </summary>
        /// <param name="version">Java的版本号</param>
        /// <returns>是否检查成功</returns>
        public static bool CheckJava(int version)
        {
            string java_path = string.Format(java_path_builder, version);
            if (!File.Exists(java_path + @"\bin\javaw.exe"))
            {
                try
                {
                    string tuna_path = $"https://mirrors.tuna.tsinghua.edu.cn/AdoptOpenJDK/{version}/jre/x64/windows/";
                    if (!Environment.Is64BitOperatingSystem) tuna_path = $"https://mirrors.tuna.tsinghua.edu.cn/AdoptOpenJDK/{version}/jre/x86/windows/";
                    string javas = HTTP.GetHttpStringData(tuna_path);
                    Regex regex = new Regex(@"<a\shref=""(?<path>.*)""\stitle="".*hotspot.*\.zip"">");
                    Match match = regex.Match(javas);
                    if (!match.Success) goto pass;
                    string filename = match.Groups["path"].ToString();
                    string remote_path = tuna_path + filename;
                    HTTP http = HTTP.CreateHTTPDownLoad(tuna_path + filename, java_path + filename);
                    http.onHttpDownloadMessage += (sender, e) => ClientMainWindow.UpdateStatusStatic(e.message, e.process);
                    while (!http.Download())
                    {
                        if (MessageBox.Show($"Java{version}下载失败了，你要重试吗？\r\n" + java_path + filename, "错误", MessageBoxButton.YesNo) == MessageBoxResult.No) goto pass;
                    }
                    ClientMainWindow.UpdateStatusStatic($"正在释放Java{version}文件...");
                    var zip = ZipFile.Open(java_path + filename, ZipArchiveMode.Read);
                    var temp_dir_name = java_path + filename.Remove(filename.LastIndexOf("."));
                    zip.ExtractToDirectory(temp_dir_name);
                    zip.Dispose();
                    FileContorl.CopyDir(Directory.EnumerateDirectories(temp_dir_name).ToArray()[0], java_path);
                    Thread.Sleep(200);
                    Directory.Delete(temp_dir_name, true);
                    File.Delete(java_path + filename);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message);
                }
                return true;
            pass:;
                return false;
            }
            else return true;
        }

        public static bool CheckJavaFX()
        {
            string java16_path = string.Format(java_path_builder, 16);
            if (!File.Exists(java16_path + @"jmods\javafx.web.jmod"))
            {
                try
                {
                    string javafx_path = "https://gluonhq.com/download/javafx-16-jmods-windows/";
                    if (!Environment.Is64BitOperatingSystem) javafx_path = "https://gluonhq.com/download/javafx-16-jmods-windows-x86/";
                    //"https://download2.gluonhq.com/openjfx/16/openjfx-16_windows-x64_bin-jmods.zip";
                    string javafx_local_path = java16_path + "javafx.zip";
                    HTTP http = HTTP.CreateHTTPDownLoad(javafx_path, javafx_local_path);
                    http.onHttpDownloadMessage += (sender, e) => ClientMainWindow.UpdateStatusStatic(e.message, e.process);
                    while (!http.Download())
                    {
                        if (MessageBox.Show("JavaFx下载失败了，你要重试吗？\r\n" + javafx_local_path, "错误", MessageBoxButton.YesNo) == MessageBoxResult.No) goto pass;
                    }
                    ClientMainWindow.UpdateStatusStatic("正在释放JavaFx文件");
                    var zip2 = ZipFile.Open(javafx_local_path, ZipArchiveMode.Read);
                    zip2.ExtractToDirectory(java16_path + "javafx");
                    zip2.Dispose();
                    if (!Directory.Exists(java16_path + "jmods")) Directory.CreateDirectory(java16_path + "jmods");
                    FileContorl.CopyDir(Directory.EnumerateDirectories(java16_path + "javafx").ToArray()[0], java16_path + "jmods");
                    Thread.Sleep(200);
                    Directory.Delete(java16_path + "javafx", true);
                    File.Delete(javafx_local_path);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message);
                    goto pass;
                }
                return true;
            pass:;
                return false;
            }
            else return true;
        }
        
        public static void CheckUpdater()
        {
            Updater updater = new Updater(App.updaterURL + "pre_updater/", App.startupPath + "Updater\\");
            updater.onUpdateMessage += (sender, e) =>
            {
                if (e.status == UpdaterEvent.MessageStatus.Download)
                {
                    ProgramWindows.BackgroundService.ShowBalloonMessage("应用程序正在更新，请稍候...");
                }
            };
            updater.Run();
        }

        //public static void GetServer1Client()
        //{
        //    try
        //    {
        //        string update_path = 
        //    }
        //}
    }
}
