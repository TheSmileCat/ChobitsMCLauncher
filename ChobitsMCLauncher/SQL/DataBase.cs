using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace ChobitsMCLauncher.SQL
{
    public class DataBase
    {
        SQLiteConnection connection;
        /// <summary>
        /// 测试用创建
        /// </summary>
        public DataBase() : this("test", false, "Data")
        {

        }
        /// <summary>
        /// 建立一个新数据库的实例
        /// </summary>
        /// <param name="databaseName">数据库的名称</param>
        /// <param name="dataFolder">是否使用全局文件夹<br/><br/>是(默认)：位于AppDatas下<br/>否：位于程序目录下<br/>null：使用绝对目录</param>
        /// <param name="path">位置</param>
        public DataBase(string databaseName, bool? dataFolder = true, params string[] path)
        {
            string connectString = "Data Source={0}";
            string dir;
            if (!dataFolder.HasValue) dir = Path.Combine(path);
            else if (dataFolder.Value == true) dir = Path.Combine(App.dataFolder, Path.Combine(path));
            else dir = Path.Combine(App.startupPath, Path.Combine(path));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, databaseName + ".sqlite");
            connection = new SQLiteConnection(string.Format(connectString, file));
            connection.Open();
        }

        public SQLiteCommand GetCommand()
        {
            return connection.CreateCommand();
        }
        
        public bool HasTable(string name)
        {
            using(SQLiteCommand command = GetCommand())
            {
                command.CommandText = "select * from 'sqlite_master' where name='" + name + "'";
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.NextResult()) return true;
                else return false;
            }
        }
        //莫名其妙会在Close();这里报错
        //~DataBase()
        //{
        //    if(connection != null)
        //    {
        //        connection.Close();
        //        connection.Dispose();
        //    }
        //}
    }
}
