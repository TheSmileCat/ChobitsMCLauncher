using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChobitsMCLauncher.Tools.CheckUpdate
{
    class ProgramEnvironmentCheck
    {
        public static void CheckNuget()
        {
            #region string[] files; //需要检测的必要文件列表 
            string[] files = new string[]
            {
                @"CefSharp.BrowserSubprocess.Core.dll",
                @"CefSharp.BrowserSubprocess.exe",
                @"CefSharp.Core.dll",
                @"CefSharp.Core.Runtime.dll",
                @"CefSharp.dll",
                @"CefSharp.Wpf.dll",
                @"ChobitsMCLauncher.exe",
                @"ChobitsMCLauncher.exe.config",
                @"chrome_100_percent.pak",
                @"chrome_200_percent.pak",
                @"chrome_elf.dll",
                @"d3dcompiler_47.dll",
                @"icudtl.dat",
                @"libcef.dll",
                @"libEGL.dll",
                @"libGLESv2.dll",
                @"md5.py",
                @"Newtonsoft.Json.dll",
                @"Panuon.UI.Silver.dll",
                @"resources.pak",
                @"snapshot_blob.bin",
                @"v8_context_snapshot.bin",
                @"app.publish\ChobitsMCLauncher.exe",
                @"byn\ChobitsMCLauncher.resources.dll",
                @"locales\en-US.pak",
                @"locales\ja.pak",
                @"locales\zh-CN.pak",
                @"locales\zh-TW.pak",
                @"swiftshader\libEGL.dll",
                @"swiftshader\libGLESv2.dll"
            };
            #endregion
            string local_path = App.startupPath;
            foreach (string s in files)
            {
                if (!File.Exists(local_path + s))
                {
                    GetNugetFile();
                    break;
                }
            }
            void GetNugetFile()
            {
                if (!ConsoleManager.IsAdministrator) ConsoleManager.RestartAsAdministrator();
                if (!Directory.Exists(local_path + "package")) Directory.CreateDirectory(local_path + "package");
                if (!File.Exists(local_path + @"package\nuget.exe")) HTTP.HttpDownload(@"https://dist.nuget.org/win-x86-commandline/v5.9.1/nuget.exe", local_path + @"package\nuget.exe");
                if (!File.Exists(local_path + @"package.config"))
                {
                    //MessageBox.Show("未能找到Nuget程序包所必须的package.config\n你可能需要重新下载客户端", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Environment.Exit(-1);
                    Assembly asm = Assembly.GetExecutingAssembly();
                    Stream sm = asm.GetManifestResourceStream("package.config");
                    byte[] bytes = new byte[sm.Length];
                    sm.Read(bytes, 0, bytes.Length);
                    File.WriteAllBytes(local_path + "package.config", bytes);
                }
                File.Copy(local_path + "package.config", local_path + @"package\package.config");
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = local_path + @"package\nuget.exe";
                processStartInfo.WorkingDirectory = local_path + "package";
                processStartInfo.Arguments = @"install packages.config -OutputDirectory .\";

            }
        }
        public static void CheckVersion()
        {
            if(App.version < int.Parse(HTTP.GetHttpStringData("http://mc.porchwood.top:3080/minecraft/updater/publish/.pubversion")))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "Updater\\ProgramPreLauncher.exe";
                processStartInfo.Verb = "runas";
                processStartInfo.UseShellExecute = false;
                processStartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "Updater";
                Process.Start(processStartInfo);
                Environment.Exit(0);
            }
        }
    }
}
