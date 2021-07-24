using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChobitsMCLauncher.Tools
{
    class HTTP
    {
        private HTTP(string url, string path, int pr_now = -1, int pr_count = -1)
        {
            this.url = url;
            this.path = path;
            this.pr_count = pr_count;
            this.pr_now = pr_now;
        }
        private int pr_now = -1;
        private int pr_count = -1;
        private string url, path;
        private static long lastUpdate = 0;
        private static ulong dataStatistics = 0;
        public static ulong GetDataStatistics()
        {
            return dataStatistics;
        }

        public static HTTP CreateHTTPDownLoad(string url, string path, int pr_now = -1, int pr_count = -1)
        {
            return new HTTP(url, path, pr_now, pr_count);
        }

        /// <summary>
        /// http下载文件
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <param name="path">文件存放地址，包含文件名</param>
        /// <returns></returns>
        public static bool HttpDownload(string url, string path, int pr_now = -1, int pr_count = -1)
        {
            HTTP http = new HTTP(url, path, pr_now, pr_count);
            return http.Download();
        }
        /// <summary>
        /// 开始下载文件
        /// </summary>
        /// <returns>下载是否成功</returns>
        public bool Download()
        {
            string tempPath = Path.GetDirectoryName(path);// + @"\temp";
            if(!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);  //创建临时文件目录
            string tempFile = tempPath + @"\" + Path.GetFileName(path) + ".temp"; //临时文件
            if (File.Exists(tempFile)) File.Delete(tempFile);
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
                responseStream.ReadTimeout = 5000;
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
            catch (Exception ex)
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
        public delegate void HttpReqRawReturn(byte[] data);
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
        /// <summary>
        /// 从网络下载一段文本
        /// </summary>
        /// <param name="path">地址</param>
        /// <param name="encode">编码</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>下载得到的文本</returns>
        public static string GetHttpStringData(string path, Encoding encode, int? timeout = null)
        {
            return GetHttpStringData(path, encode.WebName, timeout);
        }
        /// <summary>
        /// 从网络下载一段文本
        /// </summary>
        /// <param name="path">地址</param>
        /// <param name="encodeName">编码的名称</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>下载得到的文本，下载失败将返回Null</returns>
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
        private void UpdateSizeMessage(string message, ulong now, ulong count)
        {
            if (count == 0) UpdateMessage(string.Format(message, "已接收:" + toMemorySizeString(now)));
            else UpdateMessage(string.Format(message, "(" + toMemorySizeString(now) + " / " + toMemorySizeString(count) + ")"), now * 1000 / count, 1000, true);
        }
        private void UpdateMessage(string message, double now, double count, bool custom = false)
        {
            if (onHttpDownloadMessage == null) return;
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - lastUpdate > 1000000) lastUpdate = nowTicks; //100 * 10000
            else return;
            double process = now / count;
            string messageE = message;
            if (now < 0 || count < 0)
            {
                onHttpDownloadMessage(this, new HttpDownloadEventArgs(messageE));
                return;
            }
            bool a = message.Contains("{0}");
            bool b = message.Contains("{1}");
            if (custom) messageE = message;
            else if (a && b) message = string.Format(message, Math.Round(now, 2), Math.Round(count, 2));
            else if (a) message = string.Format(message, "(" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")");
            else messageE = message + " (" + Math.Round(now, 2) + " / " + Math.Round(count, 2) + ")";
            onHttpDownloadMessage(this, new HttpDownloadEventArgs(messageE, process));
        }
        private void UpdateMessage(string message)
        {
            UpdateMessage(message, -1, -1);
        }
        public string toMemorySizeString(int number)
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

        public event EventHandler<HttpDownloadEventArgs> onHttpDownloadMessage;
        public class HttpDownloadEventArgs : EventArgs
        {
            //HTTP下载过程中产生的的消息
            public string message { get; private set; }
            /// <summary>
            /// 进度，当无法显示进度时，此值为-1
            /// </summary>
            public double process { get; protected set; }
            public HttpDownloadEventArgs(string message, double process = -1)
            {
                this.message = message;
                this.process = process;
            }
        }
    }
}
