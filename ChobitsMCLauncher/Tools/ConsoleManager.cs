using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChobitsMCLauncher.Tools
{
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern int GetConsoleOutputCP();

        [DllImport("user32.dll ", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll ", EntryPoint = "RemoveMenu")]
        private static extern int RemoveMenu(IntPtr hMenu, int nPos, int flags);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern bool PostMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

        public static string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder windowName = new StringBuilder(length + 1);
            GetWindowText(hWnd, windowName, windowName.Capacity);
            return windowName.ToString();
        }

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }
        /// Creates a new console instance if the process is not attached to a console already.  
        public static void Show()
        {
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
                //找关闭按钮
                IntPtr CLOSE_MENU = GetSystemMenu(GetConsoleWindow(), IntPtr.Zero);
                int SC_CLOSE = 0xF060;
                //关闭按钮禁用
                RemoveMenu(CLOSE_MENU, SC_CLOSE, 0x0); 
            }
        }
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.   
        public static void Hide()
        {
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
        }
        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        static void InvalidateOutAndError()
        {
            Type type = typeof(Console);
            System.Reflection.FieldInfo _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.FieldInfo _error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Debug.Assert(_out != null);
            Debug.Assert(_error != null);
            Debug.Assert(_InitializeStdOutError != null);
            _out.SetValue(null, null);
            _error.SetValue(null, null);
            _InitializeStdOutError.Invoke(null, new object[] { true });
        }
        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        public static bool IsAdministrator
        {
            get
            {
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static void RestartAsAdministrator(bool needConfirm = false)
        {
            if(needConfirm == true) MessageBox.Show("即将以管理员权限启动，\n请在重启后再次尝试刚刚的操作", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            ProcessStartInfo myStartInfo = new ProcessStartInfo();
            myStartInfo.Verb = "runas";
            myStartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(myStartInfo);
            Environment.Exit(0);
        }

        public static void CreateShortcutOnDesktop()
        {
            //添加引用 (com->Windows Script Host Object Model)，using IWshRuntimeLibrary;
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ChobitsLive Minecraft.lnk");
            if (!System.IO.File.Exists(shortcutPath))
            {
                // 获取当前应用程序目录地址
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                IWshShell shell = new WshShell();
                // 确定是否已经创建的快捷键被改名了
                foreach (var item in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "*.lnk"))
                {
                    WshShortcut tempShortcut = (WshShortcut)shell.CreateShortcut(item);
                    if (tempShortcut.TargetPath == exePath)
                    {
                        return;
                    }
                }
                WshShortcut shortcut = (WshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.Arguments = "";// 参数  
                shortcut.Description = "ChobitsLive社团MC服务器客户端";
                shortcut.WorkingDirectory = Environment.CurrentDirectory;//程序所在文件夹，在快捷方式图标点击右键可以看到此属性  
                shortcut.IconLocation = exePath;//图标，该图标是应用程序的资源文件  
                //shortcut.Hotkey = "CTRL+SHIFT+W";//热键，发现没作用，大概需要注册一下  
                shortcut.WindowStyle = 1;
                shortcut.Save();
            }
        }
    }
}
