using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_MySQL_DB
    {
        public string ServerSource { set; get; }
        public string DBName { set; get; }
        public string DayOfWeed_Running { set; get; }
        public string Time_Running { set; get; }
        public List<string> Email { set; get; }


        public Exception Excute_Restore_DB(Info_MySQL_Instansce instansce, Info_FTPServer fTPServer)
        {
            try
            {
                FTPClient.Download(fTPServer.URL + "BackupDB_zip/" + DBName + "/" + DateTime.Now.ToString("yyyy-MM-dd"), fTPServer.User, fTPServer.Pass, AppDomain.CurrentDomain.BaseDirectory + @"Download\", DBName + ".zip");

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + DBName + ".sql"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + DBName + ".sql");
                }

                ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Download\" + DBName + ".zip", AppDomain.CurrentDomain.BaseDirectory);

                string constring = "server=" + instansce.Server_Name + ";port=" + instansce.Port + ";user=" + instansce.User + ";pwd=" + instansce.Pass + ";database=" + DBName + ";";
                string file = AppDomain.CurrentDomain.BaseDirectory + DBName + ".sql";
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            cmd.CommandText = "SET GLOBAL max_allowed_packet=1024*1024*1024;";
                            cmd.ExecuteNonQuery();
                            conn.Close();
                            conn.Open();
                            mb.ImportFromFile(file);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception op)
            {
                return op;
            }
            return null;

        }
    }
}
