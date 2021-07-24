using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChobitsMCLauncher.Config
{
    public class HMCLBaseConfig
    {
        public string last;
        public string backgroundType;
        public string commonDirType;
        public string commonpath;
        public string proxyType;
        public string theme;
        public string localization;
        public string downloadType;
        public string fontFamily;
        public string updateChannel;
        public string preferredLoginType;
        public bool hasProxy;
        public bool hasProxyAuth;
        public int proxyPort;
        public int logLines;
        public int _version;
        public int uiVersion;
        public double width;
        public double height;
        public double fontSize;
        public Dictionary<string, ConfigurationStruct> configurations;
        public object accounts;
        public object authlibInjectorServers;
        
        public class ConfigurationStruct
        {
            public SubStruct global;
            public string gameDir;
            public string selectedMinecraftVersion;
            public bool useRelativePath;
            public class SubStruct
            {
                public string javaArgs;
                public string minecraftArgs;
                public string permSize;
                public string javaDir;
                public string precalledCommand;
                public string serverIp;
                public string java;
                public string wrapper;
                public string gameDir;
                public string nativesDir;
                public bool usesGlobal;
                public bool fullscreen;
                public bool noJVMArgs;
                public bool notCheckGame;
                public bool notCheckJVM;
                public bool showLogs;
                public int maxMemory;
                public int width;
                public int height;
                public int launcherVisibility;
                public int gameDirType;
                public int nativesDirType;
            }
        }
        public class Account
        {
            public string uuid;
            public string username;
            public string type;
            public bool selected;
        }
    }
}
