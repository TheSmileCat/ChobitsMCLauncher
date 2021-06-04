using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChobitsMCLauncher.ProgramWindows;

namespace ChobitsMCLauncher.Tools
{
    class HTTP
    {
        private static LauncherWindow _mainWindow = null;
        private static long lastUpdate = 0;
        private static LauncherWindow mainWindow
        {
            get
            {
                return _mainWindow == null ? _mainWindow = LauncherWindow.GetWindow() : _mainWindow;
            }
        }
        private static ulong dataStatistics = 0;
        public static ulong GetDataStatistics()
        {
            return dataStatistics;
        }
        /// <summary>
        /// http下载文件
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <param name="path">文件存放地址，包含文件名</param>
        /// <returns></returns>
        public static bool HttpDownload(string url, string path, int pr_now = -1, int pr_count = -1)
        {
            string tempPath = Path.GetDirectoryName(path);// + @"\temp";
                                                          //Directory.CreateDirectory(tempPath);  //创建临时文件目录
            string tempFile = tempPath + @"\" + Path.GetFileName(path) + ".temp"; //临时文件
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);    //存在则删除
            }
            FileStream fs = null;
            int retry = 0;
            long lastpos = 0;
            long length = -1;
            ulong sum = 0;
            redo:
            try
            {
                fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                //设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.AddRange(fs.Length);
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                //response.ContentLength
                Stream responseStream = response.GetResponseStream();
                responseStream.ReadTimeout = 10000;
                if (length == -1) length = response.ContentLength;
                //创建本地文件写入流
                //Stream stream = new FileStream(tempFile, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, bArr.Length);
                dataStatistics += (ulong)size;
                sum += (ulong)size;
                while (size > 0)
                {
                    //stream.Write(bArr, 0, size);
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, bArr.Length);
                    dataStatistics += (ulong)size;
                    sum += (ulong)size;
                    if (length > 0) UpdateSizeMessage("正在下载文件 {0} " + url, sum, (ulong)length);
                    else if (pr_count != -1 && pr_count != -1) UpdateMessage("正在下载文件[" + toMemorySizeString(sum) + "] {0} " + url, pr_now, pr_count);
                    else UpdateMessage("正在下载文件[" + toMemorySizeString(sum) + "] " + url);
                }
                //stream.Close();
                fs.Close();
                responseStream.Close();
                if (File.Exists(path)) File.Delete(path);
                File.Move(tempFile, path);
                return true;
            }
            catch (Exception)
            {
                if (lastpos == fs.Length) retry++;
                else retry = 0;
                lastpos = fs.Length;
                if (retry > 3) return false;
                else goto redo;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        public static void GetHttpData(HttpReqRawReturn dataReturn, string path, int? timeout = null)
        {
            HttpWebRequest request = WebRequest.Create(path) as HttpWebRequest;
            if (timeout.HasValue) request.Timeout = timeout.Value;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            //StreamReader sr = new StreamReader(responseStream);
            //sr.ReadToEnd()
            List<byte> data = new List<byte>();
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, bArr.Length);
            dataStatistics += (ulong)size;
            while (size > 0)
            {
                data.AddRange(bArr);
                size = responseStream.Read(bArr, 0, bArr.Length);
                dataStatistics += (ulong)size;
            }
            responseStream.Close();
            dataReturn(data.ToArray());
        }
        public static string GetHttpStringData(string path, Encoding encode, int? timeout = null)
        {
            return GetHttpStringData(path, encode.WebName, timeout);
        }
        public static string GetHttpStringData(string path, string encodeName = "UTF-8", int? timeout = null)
        {
            string s = null;
            try
            {
                //GetHttpData((data) => s = Encoding.GetEncoding(encodeName).GetString(data), path, timeout);
                HttpWebRequest request = WebRequest.Create(path) as HttpWebRequest;
                if (timeout.HasValue) request.Timeout = timeout.Value;
                //dataStatistics += (ulong)(request.ContentLength == -1 ? 0 : request.ContentLength);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream, Encoding.GetEncoding(encodeName));
                s = sr.ReadToEnd();
                sr.Close();
                dataStatistics += (ulong)Encoding.Default.GetByteCount(s);
            }
            catch
            {
                return null;
            }
            return s;
        }
        public delegate void HttpReqRawReturn(byte[] data);
        private static void UpdateSizeMessage(string message, ulong now, ulong count)
        {
            if (count == 0) UpdateMessage(string.Format(message, "已接收:" + toMemorySizeString(now)));
            else UpdateMessage(string.Format(message, "(" + toMemorySizeString(now) + " / " + toMemorySizeString(count)) + ")", now * 1000 / count, 1000, true);
        }
        private static void UpdateMessage(string message, double now, double count, bool custom = false)
        {
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - lastUpdate > 1000000) lastUpdate = nowTicks; //100 * 10000
            else return;
            if (now < 0 || count < 0)
            {
                mainWindow.UpdateStatus(message, -1);
                return;
            }
            bool a = message.Contains("{0}");
            bool b = message.Contains("{1}");
            if (custom) mainWindow.UpdateStatus(message, now / count);
            else if (a && b) mainWindow.UpdateStatus(string.Format(message, Math.Round(now, 2), Math.Round(count, 2)), now / count);
            else if (a) mainWindow.UpdateStatus(string.Format(message, "(" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")"), now / count);
            else mainWindow.UpdateStatus(message + " (" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")", now / count);
        }
        private static void UpdateMessage(string message)
        {
            UpdateMessage(message, -1, -1);
        }
        public static string toMemorySizeString(int number)
        {
            return toMemorySizeString((ulong)number);
        }
        public static string toMemorySizeString(long number)
        {
            return toMemorySizeString((ulong)number);
        }
        public static string toMemorySizeString(ulong number)
        {
            string s = "";
            if (number < 1024) s = number + "b";
            else if (number < 1048576) s = (double)(number * 10 / 1024) / 10 + "kb";          //1024^2=1048576
            else if (number < 1073741824) s = (double)(number * 10 / 1048576) / 10 + "Mb";    //1024^3=1073741824
            else s = (double)(number * 10 / 1073741824) / 10 + "Gb";
            return s;
        }
    }
}
