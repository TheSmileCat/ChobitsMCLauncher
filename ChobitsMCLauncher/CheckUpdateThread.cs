using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using ChobitsMCLauncher.ProgramWindows;

namespace ChobitsMCLauncher
{
    class CheckUpdateThread
    {
        private static LauncherWindow mainWindow = null;
        private static int redoCount = 0;
        private static List<string> controlFiles;
        private static string area = null;
        public static void Run()
        {
            redo:
            mainWindow = LauncherWindow.GetWindow();
            controlFiles = new List<string>();
            if (redoCount > 1)
            {
                MessageBoxResult result = MessageBox.Show(
                    "程序发生了多次启动重试，确定要继续重试吗？\r\n[是]继续\t[否]启动游戏\t[取消]关闭启动器",
                    "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) Environment.Exit(0);
                else if (result == MessageBoxResult.No) goto launch;
            }
            //程序启动“块”
            {
                try
                {
                    area = JsonConvert.DeserializeObject<string>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "../area.json"));
                    if (area == "" || area == null) throw new Exception("找不到area.json配置文件或配置文件异常");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message);
                    goto launch;
                }
                string s = Tools.HTTP.GetHttpStringData("http://chobitslive.live:3080/minecraft/updater/" + area + "/version.json", timeout: 2000);
                if (s == null) Tools.HTTP.GetHttpStringData("http://chobitslive.live:3080/minecraft/updater/version.json", timeout: 2000);
                if (s == null)
                {
                    mainWindow.InternetAbort();
                    return;
                }
                else
                {
                    try
                    {
                        var obj = JsonConvert.DeserializeObject(s) as JObject;
                        string v = obj.GetValue("version_string").ToObject<string>();
                        mainWindow.SetDisplayVersion(v);
                        int w = obj.GetValue("width").ToObject<int>();
                        int h = obj.GetValue("height").ToObject<int>();
                        mainWindow.ChangeWindowWH(w, h);
                        mainWindow.SetBackgroundWebAddress(obj.GetValue("link").ToObject<string>());
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.StackTrace, e.Message);
                    }
                }
            }
            while (!mainWindow.GetIsLoaded()) Thread.Sleep(250);
            //游戏更新“块”
            {
                string control_file_s = Tools.HTTP.GetHttpStringData("http://chobitslive.live:3080/minecraft/updater/control.json");
                if (control_file_s != null)
                {
                    try
                    {
                        JArray jArray = JsonConvert.DeserializeObject(control_file_s) as JArray;
                        string[] cfs = jArray.ToObject<string[]>();
                        controlFiles.AddRange(cfs);
                    }
                    catch { }
                }
                int filed = 0;
                int done = 0;
                string foldsRaw = Tools.HTTP.GetHttpStringData("http://chobitslive.live:3080/minecraft/updater/" + area + "/minecraft/folds.json", timeout: 30000);
                string[] folds = null;
                if (foldsRaw == null)
                {
                    mainWindow.InternetAbort();
                    return;
                }
                else
                {
                    try
                    {
                        JArray obj = JsonConvert.DeserializeObject(foldsRaw) as JArray;
                        folds = obj.ToObject<string[]>();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.StackTrace, e.Message);
                    }
                }
                if (folds != null)
                {
                    foreach (string f in folds)
                    {
                        string local_path = (AppDomain.CurrentDomain.BaseDirectory + "../.minecraft/" + f).Replace("/", "\\") + "\\";
                        string custom_path = (AppDomain.CurrentDomain.BaseDirectory + "../.customfiles/" + f).Replace("/", "\\") + "\\";
                        string internet_path = "http://chobitslive.live:3080/minecraft/updater/" + area + "/minecraft/" + f.Replace("\\", "/") + "/";
                        string folder_setting = internet_path + "fold.json";
                        try
                        {
                            string s = Tools.HTTP.GetHttpStringData(folder_setting);
                            var obj = JsonConvert.DeserializeObject(s) as JObject;
                            string type = obj.GetValue("type").ToObject<string>();
                            if (type == "Hash")
                            {
                                string md5sPath = internet_path + "md5s.json";
                                string md5s = Tools.HTTP.GetHttpStringData(md5sPath);
                                Dictionary<string, string> dict_internet_files = JsonConvert.DeserializeObject<Dictionary<string, string>>(md5s);
                                //Dictionary<string, string> dict_internet_files = filelist.ToObject<Dictionary<string, string>>();
                                Dictionary<string, string> dict_local_files = new Dictionary<string, string>();
                                Dictionary<string, string> dict_custom_files = new Dictionary<string, string>();
                                if (!Directory.Exists(local_path)) Directory.CreateDirectory(local_path);
                                if (!Directory.Exists(custom_path)) Directory.CreateDirectory(custom_path);
                                string[] localFiles = Directory.GetFiles(local_path);
                                string[] customFiles = Directory.GetFiles(custom_path);
                                for (int i = 0; i < customFiles.Length; i++)
                                {
                                    UpdateMessage("计算本地自定义文件哈希值 {0} " + customFiles[i], i + 1, customFiles.Length);
                                    dict_custom_files.Add(new FileInfo(customFiles[i]).Name, Tools.Md5.GetMD5HashFromFile(customFiles[i]));
                                }
                                for (int i = 0; i < localFiles.Length; i++)
                                {
                                    UpdateMessage("计算本地文件哈希值 {0} " + localFiles[i], i + 1, localFiles.Length);
                                    string local_hash = Tools.Md5.GetMD5HashFromFile(localFiles[i]);
                                    dict_local_files.Add(new FileInfo(localFiles[i]).Name, local_hash);
                                }
                                UpdateMessage("正在由本地向上匹配文件，请稍候...");
                                //步骤1 本地向上检查
                                foreach (KeyValuePair<string, string> local in dict_local_files)
                                {
                                    try
                                    {
                                        if (!dict_internet_files.ContainsValue(local.Value) && !dict_custom_files.ContainsValue(local.Value))
                                        {
                                            File.Delete(local_path + local.Key);
                                            done++;
                                        }
                                    }
                                    catch (IOException e)
                                    {
                                        filed++;
                                    }
                                }
                                UpdateMessage("正在由远程向下匹配文件，请稍候...");
                                //步骤2 远程向下检查
                                int dict_internet_files_enum_now = 0;
                                foreach (KeyValuePair<string, string> internet in dict_internet_files)
                                {
                                    dict_internet_files_enum_now++;
                                    if (IsControlFile(internet.Key)) continue;
                                    if (!dict_local_files.ContainsValue(internet.Value))
                                    {
                                        if (Tools.HTTP.HttpDownload(internet_path + internet.Key, local_path + internet.Key, dict_internet_files_enum_now, dict_internet_files.Count) == false) filed++;
                                        done++;
                                    }
                                }
                                UpdateMessage("正在进行自定义文件的匹配，请稍候...");
                                int dict_custom_files_enum_now = 0;
                                //步骤3 本地检查
                                foreach (KeyValuePair<string, string> custom in dict_custom_files)
                                {
                                    dict_custom_files_enum_now++;
                                    if (!dict_local_files.ContainsValue(custom.Value))
                                    {
                                        UpdateMessage("正在复制文件 {0} " + custom_path + custom.Key, dict_custom_files_enum_now, dict_custom_files.Count);
                                        try
                                        {
                                            if (File.Exists(local_path + custom.Key)) File.Delete(local_path + custom.Key);
                                            File.Copy(custom_path + custom.Key, local_path + custom.Key);
                                            done++;
                                        }
                                        catch
                                        {
                                            filed++;
                                        }
                                    }
                                }
                            }
                            else if (type == "loose")
                            {
                                string md5sPath = internet_path + "md5s.json";
                                string md5s = Tools.HTTP.GetHttpStringData(md5sPath);
                                Dictionary<string, string> dict_internet_files = JsonConvert.DeserializeObject<Dictionary<string, string>>(md5s);
                                UpdateMessage("正在由远程向下匹配文件，请稍候...");
                                //步骤2 远程向下检查
                                int dict_internet_files_enum_now = 0;
                                Dictionary<string, string> dict_local_files = new Dictionary<string, string>();
                                string[] localFiles = Directory.GetFiles(local_path);
                                for (int i = 0; i < localFiles.Length; i++)
                                {
                                    UpdateMessage("计算本地文件哈希值 {0} " + localFiles[i], i + 1, localFiles.Length);
                                    string local_hash = Tools.Md5.GetMD5HashFromFile(localFiles[i]);
                                    dict_local_files.Add(new FileInfo(localFiles[i]).Name, local_hash);
                                }
                                foreach (KeyValuePair<string, string> internet in dict_internet_files)
                                {
                                    dict_internet_files_enum_now++;
                                    if (IsControlFile(internet.Key)) continue;
                                    if (!dict_local_files.ContainsKey(internet.Key))
                                    {
                                        if (Tools.HTTP.HttpDownload(internet_path + internet.Key, local_path + internet.Key, dict_internet_files_enum_now, dict_internet_files.Count) == false) filed++;
                                        done++;
                                    }
                                    else if (!dict_local_files.ContainsValue(internet.Value))
                                    {
                                        if (Tools.HTTP.HttpDownload(internet_path + internet.Key, local_path + internet.Key, dict_internet_files_enum_now, dict_internet_files.Count) == false) filed++;
                                        done++;
                                    }
                                }
                                UpdateMessage("正在由远程向下请求删除文件，请稍候...");
                                string fo_delete_path = internet_path + "delete.json";
                                string fo_delete_s = Tools.HTTP.GetHttpStringData(fo_delete_path);
                                if (fo_delete_s != null)
                                {
                                    JArray file_list = JsonConvert.DeserializeObject(fo_delete_s) as JArray;
                                    string[] files = file_list.ToObject<string[]>();
                                    foreach (string singal_file in files)
                                    {
                                        try
                                        {
                                            if (File.Exists(local_path + singal_file)) File.Delete(local_path + singal_file);
                                            done++;
                                        }
                                        catch
                                        {
                                            filed++;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.StackTrace, e.Message);
                        }
                    }
                    UpdateMessage("结束操作……检查文件状态");
                    if (filed > 0)
                    {
                        if (MessageBox.Show("有" + filed + "和文件操作失败了，你要重试一下吗？", "异常", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                        {
                            redoCount++;
                            goto redo;
                        }
                    }
                }
                if (done > 0)
                {
                    redoCount++;
                    goto redo;
                }
            }
            //启动游戏启动器 块
            launch:
            {
                string Java = AppDomain.CurrentDomain.BaseDirectory + "../.java/bin/javaw.exe";
                if (!File.Exists(Java)) Java = "javaw.exe";
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = Java;
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "../";
                process.StartInfo.Arguments = "-jar Launcher.jar";
                process.Start();
                UpdateMessage("正在等待启动器启动...");
                Thread.Sleep(3000);
                LauncherWindow.GetWindow().CloseIt();
            }
        }
        private static void UpdateMessage(string message, double now, double count)
        {
            bool a = message.Contains("{0}");
            bool b = message.Contains("{1}");
            if (a && b) mainWindow.UpdateStatus(string.Format(message, Math.Round(now, 2), Math.Round(count, 2)), now / count);
            else if (a) mainWindow.UpdateStatus(string.Format(message, "(" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")"), now / count);
            else mainWindow.UpdateStatus(message + " (" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")", now / count);
        }
        private static void UpdateMessage(string message)
        {
            mainWindow.UpdateStatus(message, -1);
        }
        private static bool IsControlFile(string fileName)
        {
            //switch (fileName)
            //{
            //    case "md5s.json":
            //    case "md5.py":
            //    case "fold.json":
            //    case "folds.json":
            //    case "delete.json":
            //        return true;
            //    default:
            //        return false;
            //}
            return controlFiles.Contains(fileName);
        }
    }
}
