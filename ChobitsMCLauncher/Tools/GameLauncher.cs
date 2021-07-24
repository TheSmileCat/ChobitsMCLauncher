using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ChobitsMCLauncher.ProgramWindows.Client;
using ChobitsMCLauncher.Tools.CheckUpdate;
using Newtonsoft.Json;
using Panuon.UI.Silver;

namespace ChobitsMCLauncher.Tools
{
    class GameLauncher
    {
        static string local_path = AppDomain.CurrentDomain.BaseDirectory;
        static string java_path_builder = local_path + @"Java\Java{0}\bin\javaw.exe";
        static string gameroot_path = local_path + @"Games\";
        public static bool isFirstLaunch(int javaVersion)
        {
            if(!File.Exists(App.dataFolder + $"java{javaVersion}.launched"))
            {
                File.WriteAllText(App.dataFolder + $"java{javaVersion}.launched", DateTime.Now.ToString());
                return true;
            }
            return false;
        }

        private static bool LaunchClient(string path, int javaVersion)
        {
            Config.HMCLBaseConfig config = JsonConvert.DeserializeObject<Config.HMCLBaseConfig>(File.ReadAllText(path + "hmcl.json"));
            config.configurations["Default"].global.java = "Custom";
            config.configurations["Default"].global.javaDir = string.Format(java_path_builder, javaVersion).Replace("javaw.exe", "java.exe");
            config.configurations["Default"].global.serverIp = "";//"chobitslive.live:1152"; 会导致崩溃
            if (!Environment.Is64BitOperatingSystem) config.configurations["Default"].global.maxMemory = 1024;
            File.Delete(path + "hmcl.json");
            File.WriteAllText(path + "hmcl.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.Arguments = "-jar \"" + path + "Launcher.jar\"";
            processStartInfo.WorkingDirectory = path;
            processStartInfo.FileName = string.Format(java_path_builder, 16);
            if (Process.Start(processStartInfo) == null)
            {
                MessageBox.Show("HMCL启动失败");
                if (ClientMainWindow.GetWindow() != null) ClientMainWindow.GetWindow().UpdateButton("开始游戏", false);
                return false;
            }
#if DEBUG
            else if (MessageBox.Show("操作结束，退出吗？", "询问", MessageBoxButton.YesNo) == MessageBoxResult.Yes) Environment.Exit(0);
            ClientMainWindow.UpdateStatusStatic("", 1);
            ClientMainWindow.UpdateButtonStatic(" 开始游戏", false);
#else
            ClientMainWindow.UpdateStatusStatic("正在启动HMCL启动器...");
            if(isFirstLaunch(javaVersion)) MessageBox.Show($"如果你是笔记本用户，首次启动建议在英伟达控制面板中将“{processStartInfo.FileName}”改为使用“高性能”(独立显卡)来运行游戏，可获得极大性能提升", "建议");
            //if (!File.Exists(local_path + ".dontexit")) Environment.Exit(0);
#endif
            return true;
        }

        public static bool Server1ClientCheckUpdate()
        {
            string localPath = gameroot_path + @"UniversalClient\.minecraft\";
            string internetPath = App.updaterURL + @"1st/minecraft/";
            string customPath = gameroot_path + @"UniversalClient\自定义文件目录\";
            Updater updater = new Updater(internetPath, localPath, customPath);
            bool isFailed = false;
            updater.onUpdateMessage += (sender, e) =>
            {
                if (e.status == UpdaterEvent.MessageStatus.Info)
                {
                    ClientMainWindow.UpdateStatusStatic(e.message, e.process);
                }
                else if (e.status == UpdaterEvent.MessageStatus.Failed) isFailed = true;
            };
            ClientMainWindow.UpdateButtonStatic("正在检查更新", true);
            updater.Run();
            return !isFailed;
        }

        public static bool DoServer1ClientLaunch()
        {
            string uni_version_path = gameroot_path + @"UniversalClient\";
            if (!Server1ClientCheckUpdate()) return false;
            ClientMainWindow.GetWindow().UpdateButton("正在启动", true);
            return LaunchClient(uni_version_path, 16);
        }

        public static bool Download3rdClient()
        {
            ClientMainWindow.UpdateButtonStatic("正在获取客户端", true);
            try
            {
                string path = gameroot_path + "3rdServerClient/";
                string archiveFullName = gameroot_path + "3rdServerClient.zip";
                HTTP http = HTTP.CreateHTTPDownLoad(App.updaterURL + "3rd/archive.zip", archiveFullName);
                http.onHttpDownloadMessage += (sender, e) => ClientMainWindow.UpdateStatusStatic(e.message, e.process);
                if (!http.Download()) return false;
                var zip = ZipFile.Open(archiveFullName, ZipArchiveMode.Read);
                zip.ExtractToDirectory(path);
                zip.Dispose();
                Thread.Sleep(200);
                File.Delete(archiveFullName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
                return false;
            }
        }

        public static bool Server3ClientCheckUpdate()
        {
            string localPath = gameroot_path + @"3rdServerClient\.minecraft\";
            string internetPath = App.updaterURL + @"3rd/minecraft/";
            string customPath = gameroot_path + @"3rdServerClient\自定义文件目录\";
            Updater updater = new Updater(internetPath, localPath, customPath);
            bool isFailed = false;
            updater.onUpdateMessage += (sender, e) =>
            {
                if (e.status == UpdaterEvent.MessageStatus.Info)
                {
                    ClientMainWindow.UpdateStatusStatic(e.message, e.process);
                }
                else if (e.status == UpdaterEvent.MessageStatus.Failed) isFailed = true;
            };
            ClientMainWindow.UpdateButtonStatic("正在检查更新", true);
            updater.Run();
            return !isFailed;
        }

        public static bool DoServer3ClientLaunch()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show(
                    "你的电脑达不到游玩该客户端所必须的最低标准： 64位操作系统\n如果真的想玩，就去刷机吧……", 
                    "错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return false;
            }
            string client3Path = gameroot_path + @"3rdServerClient\";
            if (!PreLaunch.CheckJava(11)) return false;
            if (!File.Exists(client3Path + "Launcher.jar")) if(!Download3rdClient()) return false;
            if (!Server3ClientCheckUpdate()) return false;
            return LaunchClient(client3Path, 11);
        }
    }
}
