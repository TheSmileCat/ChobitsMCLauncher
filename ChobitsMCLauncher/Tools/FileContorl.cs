using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChobitsMCLauncher.Tools
{
    class FileContorl
    {
        public static void CopyDir(string origin, string target)
        {
            try
            {
                if (!origin.EndsWith("\\")) origin += "\\";
                if (!target.EndsWith("\\")) target += "\\";
                DirectoryInfo info = new DirectoryInfo(origin);
                if (!Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                }

                FileInfo[] fileList = info.GetFiles();
                DirectoryInfo[] dirList = info.GetDirectories();
                foreach (FileInfo fi in fileList)
                {
                    File.Copy(fi.FullName, target + fi.Name, true);
                }
                foreach (DirectoryInfo di in dirList)
                {
                    CopyDir(di.FullName, target + "\\" + di.Name);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("复制文件时发生错误\r\n源：" + origin + "，目标：" + target + "\r\n" + e.Message);
                throw e;
            }
        }

        public static void CopyDirWithOriginName(string origin, string target)
        {

            if (File.Exists(origin))
            {
                if (!Directory.Exists(target)) Directory.CreateDirectory(target);
                try
                {
                    File.Copy(origin, target + "\\" + origin.Substring(origin.LastIndexOf("\\")), true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("复制文件时发生错误\r\n源：" + origin + "，目标：" + target + "\r\n" + e.Message);
                    throw e;
                }
            }
            else
            {
                string targetPath = target.EndsWith("\\") ? target.Substring(0, target.Length - 1) : target;
                targetPath += "\\" + new DirectoryInfo(origin).Name + "\\";
                CopyDir(origin, targetPath);
            }
        }

    }
}
