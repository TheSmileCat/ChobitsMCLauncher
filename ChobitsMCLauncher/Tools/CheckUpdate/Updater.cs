using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChobitsMCLauncher.Tools.CheckUpdate
{
    class Updater
    {
        public static string host = App.hostURL;
        public Updater(string internetPath, string folder)
        {
            init();
            if (!internetPath.EndsWith("/")) internetPath += "/";
            this.internetPath = internetPath;
            if (!folder.EndsWith("\\")) folder += "\\";
            this.localFolder = folder;
        }

        public Updater(string internetPath, string folder, string customFolder) : this(internetPath, folder)
        {
            this.customFolder = customFolder;
        }

        public event EventHandler<UpdaterEvent> onUpdateMessage;
        private static bool inited = false;
        private static void init()
        {
            if (inited) return; 
            string control_file_s = HTTP.GetHttpStringData(host + "minecraft/updater/control.json");
            if (control_file_s != null)
            {
                try
                {
                    JArray jArray = JsonConvert.DeserializeObject(control_file_s) as JArray;
                    string[] cfs = jArray.ToObject<string[]>();
                    gobalControlFiles.AddRange(cfs);
                }
                catch { }
            }

        }

        private int redoCount = 0;
        private List<string> controlFiles;
        private static List<string> gobalControlFiles = new List<string>();
        private string internetPath = null;
        private string localFolder = null;
        private string customFolder = null;
        private bool downloaded = false;
        private void SendDownloadMessage()
        {
            if (downloaded) return;
            downloaded = true;
            if (onUpdateMessage != null) onUpdateMessage(this, new UpdaterEvent("开始下载文件了", UpdaterEvent.MessageStatus.Download));
        }

        public Task RunAsync()
        {
            return Task.Run(Run);
        }

        /// <summary>
        /// 开始检查更新
        /// </summary>
        public void Run()
        {
            redoCount = 0;
        redo:
            controlFiles = new List<string>();
            if (redoCount > 1)
            {
                MessageBoxResult result = MessageBox.Show(
                    "更新组件在检查更新时发生了多次启动重试，确定要继续重试吗？\r\n[是]继续\t[否]启动游戏",
                    "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) goto launch;
            }
            //开始更新文件夹
            {
                //
                //从host获取控制文件列表
                string control_file_s = HTTP.GetHttpStringData(internetPath + "control.json");
                if (control_file_s == null) controlFiles = gobalControlFiles;
                int filed = 0;
                int done = 0;
                //
                //获取目标文件夹
                string foldsRaw = HTTP.GetHttpStringData(internetPath + "folds.json", timeout: 30000);
                //
                //所有目标文件夹
                string[] folds = null;
                if (foldsRaw == null)
                {
                    //如果获取到的文件夹为空，返回
                    return;
                }
                else
                {
                    try
                    {
                        //处理文件夹
                        JArray obj = JsonConvert.DeserializeObject(foldsRaw) as JArray;
                        folds = obj.ToObject<string[]>();
                    }
                    catch (Exception e)
                    {
                        new Thread(() => MessageBox.Show(e.StackTrace, e.Message)).Start();
                    }
                }
                //
                //获取到的文件夹为空时返回
                if (folds == null) return;
                foreach (string f in folds)
                {
                    //循环遍历文件夹，并下载其中的文件
                    string local_path = (localFolder + f).Replace("/", "\\");
                    if (!local_path.EndsWith("\\")) local_path += "\\";
                    string internet_path = internetPath + f.Replace("\\", "/");
                    if (!internet_path.EndsWith("/")) internet_path += "/";
                    string custom_path = null;
                    if (customFolder != null)
                    {
                        custom_path = (customFolder + f).Replace("/", "\\");
                        if (!custom_path.EndsWith("\\")) custom_path += "\\";
                    }
                    filed += downloadFilesForFolder(internet_path, local_path, custom_path);
                }
                UpdateMessage("结束操作……检查文件状态");
                if (filed > 0)
                {
                    if (MessageBox.Show("有" + filed + "次操作失败了，你要重试一下吗？", "异常", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                    {
                        redoCount++;
                        goto redo;
                    }
                }
                if (done > 0)
                {
                    redoCount++;
                    goto redo;
                }
            }
        //启动结束操作 块
        launch:
            {
                if (onUpdateMessage != null) onUpdateMessage(this, new UpdaterEvent("操作结束", UpdaterEvent.MessageStatus.Done));
            }
        }
        /// <summary>
        /// 为一个目标文件夹更新内容
        /// </summary>
        /// <param name="internetPath">目标网络文件夹地址</param>
        /// <param name="localPath">目标本地文件夹地址</param>
        private int downloadFilesForFolder(string internetPath, string localPath, string customPath = null)
        {
            int failedCount = 0;
            string folder_setting = internetPath + "fold.json";
            try
            {
                string s = HTTP.GetHttpStringData(folder_setting);
                string type = "Hash";
                if (s != null)
                {
                    var obj = JsonConvert.DeserializeObject(s) as JObject;
                    type = obj.GetValue("type").ToObject<string>();
                }
                if (type == "Hash")
                {
                    string md5sPath = internetPath + "md5s.json";
                    string md5s = HTTP.GetHttpStringData(md5sPath);
                    if(md5s == null)
                    {
                        Task.Run(() => MessageBox.Show("远程服务器配置错误，md5s.json不存在，位于\n" + md5sPath));
                    }
                    //网络文件列表
                    Dictionary<string, string> dict_internet_files = JsonConvert.DeserializeObject<Dictionary<string, string>>(md5s);
                    //夜长梦多，从这里移除控制文件（出BUG了）
                    foreach(string controlFile in controlFiles) if (dict_internet_files.ContainsKey(controlFile)) dict_internet_files.Remove(controlFile);
                    //本地文件列表
                    Dictionary<string, string> dict_local_files = new Dictionary<string, string>();
                    //自定义文件列表
                    Dictionary<string, string> dict_custom_files = new Dictionary<string, string>();
                    if (!Directory.Exists(localPath)) Directory.CreateDirectory(localPath);
                    string[] localFiles = Directory.GetFiles(localPath);
                    //自定义文件不启动时不检测和创建自定义目录
                    if (customPath != null && !Directory.Exists(customPath)) Directory.CreateDirectory(customPath);
                    string[] customFiles = null;
                    if (customPath != null)
                    {
                        customFiles = Directory.GetFiles(customPath);
                        for (int i = 0; i < customFiles.Length; i++)
                        {
                            UpdateMessage("计算本地自定义文件哈希值 {0} " + customFiles[i], i + 1, customFiles.Length);
                            dict_custom_files.Add(new FileInfo(customFiles[i]).Name, Md5.GetMD5HashFromFile(customFiles[i]));
                        }
                    }
                    //本地文件的哈希值检测
                    for (int i = 0; i < localFiles.Length; i++)
                    {
                        UpdateMessage("计算本地文件哈希值 {0} " + localFiles[i], i + 1, localFiles.Length);
                        string local_hash = Md5.GetMD5HashFromFile(localFiles[i]);
                        dict_local_files.Add(new FileInfo(localFiles[i]).Name, local_hash);
                    }
                    UpdateMessage("正在由本地向上匹配文件，请稍候...");
                    //步骤1 本地向上检查
                    foreach (KeyValuePair<string, string> local in dict_local_files)
                    {
                        try
                        {
                            //如果自定义文件和远程文件皆没有这个文件，删除
                            if (!(dict_internet_files.ContainsValue(local.Value) || dict_custom_files.ContainsValue(local.Value)))
                            {
                                File.Delete(localPath + local.Key);
                            }
                            //当文件名不匹配时
                            else if (!dict_internet_files.ContainsKey(local.Key))
                            {
                                foreach (KeyValuePair<string, string> kv in dict_internet_files)
                                {
                                    if (kv.Value == local.Value)
                                    {
                                        File.Move(local.Key, kv.Key);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            new Thread(() => MessageBox.Show(e.StackTrace, e.Message)).Start();
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
                            SendDownloadMessage();
                            HTTP http = HTTP.CreateHTTPDownLoad(
                                internetPath + internet.Key,
                                localPath + internet.Key,
                                dict_internet_files_enum_now,
                                dict_internet_files.Count);
                            http.onHttpDownloadMessage += (sender, e) =>
                            {
                                UpdateMessage(e.message, e.process, 1);
                            };
                            if (!http.Download()) failedCount++;
                        }
                    }
                    //
                    //向自定义文件只在CustomFiles启动时可用
                    if (customFiles != null)
                    {
                        UpdateMessage("正在进行自定义文件的匹配，请稍候...");
                        int dict_custom_files_enum_now = 0;
                        //步骤3 本地检查
                        foreach (KeyValuePair<string, string> custom in dict_custom_files)
                        {
                            dict_custom_files_enum_now++;
                            if (!dict_local_files.ContainsValue(custom.Value))
                            {
                                UpdateMessage("正在复制文件 {0} " + customPath + custom.Key, dict_custom_files_enum_now, dict_custom_files.Count);
                                try
                                {
                                    if (File.Exists(localPath + custom.Key)) File.Delete(localPath + custom.Key);
                                    File.Copy(customPath + custom.Key, localPath + custom.Key);
                                }
                                catch(Exception e)
                                {
                                    new Thread(() => MessageBox.Show(e.StackTrace, e.Message)).Start();
                                    failedCount++;
                                }
                            }
                        }
                    }
                }
                else if (type == "loose")
                {
                    string md5sPath = internetPath + "md5s.json";
                    string md5s = HTTP.GetHttpStringData(md5sPath);
                    Dictionary<string, string> dict_internet_files = JsonConvert.DeserializeObject<Dictionary<string, string>>(md5s);
                    UpdateMessage("正在由远程向下匹配文件，请稍候...");
                    //步骤2 远程向下检查
                    int dict_internet_files_enum_now = 0;
                    Dictionary<string, string> dict_local_files = new Dictionary<string, string>();
                    string[] localFiles = Directory.GetFiles(localPath);
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
                        if (!dict_local_files.ContainsKey(internet.Key) || !dict_local_files.ContainsValue(internet.Value))
                        {
                            try
                            {
                                SendDownloadMessage();
                                if (File.Exists(internet.Key)) File.Delete(internet.Key);
                                HTTP http = HTTP.CreateHTTPDownLoad(
                                    internetPath + internet.Key,
                                    localPath + internet.Key,
                                    dict_internet_files_enum_now,
                                    dict_internet_files.Count);
                                http.onHttpDownloadMessage += (sender, e) =>
                                {
                                    UpdateMessage(e.message, e.process, 1);
                                };
                                if (!http.Download()) failedCount++;
                            }
                            catch(Exception e)
                            {
                                new Thread(() => MessageBox.Show(e.StackTrace, e.Message)).Start();
                                failedCount++;
                            }
                        }
                    }
                    UpdateMessage("正在由远程向下请求删除文件，请稍候...");
                    string fo_delete_path = internetPath + "delete.json";
                    string fo_delete_s = Tools.HTTP.GetHttpStringData(fo_delete_path);
                    if (fo_delete_s != null)
                    {
                        JArray file_list = JsonConvert.DeserializeObject(fo_delete_s) as JArray;
                        string[] files = file_list.ToObject<string[]>();
                        foreach (string singal_file in files)
                        {
                            try
                            {
                                if (File.Exists(localPath + singal_file)) File.Delete(localPath + singal_file);
                                //done++;
                            }
                            catch(Exception e)
                            {
                                //filed++;
                                new Thread(() => MessageBox.Show(e.StackTrace, e.Message)).Start();
                                failedCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                new Thread(() => MessageBox.Show(e.StackTrace, e.Message)).Start();
                failedCount++;
            }
            return failedCount;
        }

        private void UpdateMessage(string message, double now, double count)
        {
            bool a = message.Contains("{0}");
            bool b = message.Contains("{1}");
            string messageE;
            if (a && b) messageE = string.Format(message, Math.Round(now, 2), Math.Round(count, 2));
            else if (a) messageE = string.Format(message, "(" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")");
            else messageE = message + " (" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")";
            if (onUpdateMessage != null) onUpdateMessage(this, new UpdaterEvent(now, count, messageE));
        }
        private void UpdateMessage(string message)
        {
            if (onUpdateMessage != null) onUpdateMessage(this, new UpdaterEvent(message));
        }
        private bool IsControlFile(string fileName)
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
