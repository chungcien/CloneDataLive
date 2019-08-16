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
        public string DBSource { set; get; }
        public string DBTarget { set; get; }
        public string DayOfWeed_Running { set; get; }
        public string Time_Running { set; get; }
        public List<string> Email { set; get; }


        public Exception Download_BackupFile(Info_FTPServer fTPServer)
        {
            try
            {
                File_Read_Write.Create_Exits_Folder(AppDomain.CurrentDomain.BaseDirectory + @"Download\");

                bool isDown = FTPClient.Download(fTPServer.URL + "BackupDB_zip/" + ServerSource + "/" + DateTime.Now.ToString("yyyy-MM-dd"), fTPServer.User, fTPServer.Pass, AppDomain.CurrentDomain.BaseDirectory + @"Download\", DBSource + ".zip");

                if (isDown != true)
                {
                    return new Exception("Download Backup file failed!", new Exception(""));
                }

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + DBSource + ".sql"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + DBSource + ".sql");
                }

                ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Download\" + DBSource + ".zip", AppDomain.CurrentDomain.BaseDirectory);

            }
            catch (Exception op)
            {
                return new Exception(op.Message, op.InnerException);
            }
            return null;
        }



        public Exception Excute_Restore_DB(Info_MySQL_Instansce instansce)
        {
            try
            {
                
                string constring = "server=" + instansce.Server_Name + ";port=" + instansce.Port + ";user=" + instansce.User + ";pwd=" + instansce.Pass + ";";
                string file = AppDomain.CurrentDomain.BaseDirectory + DBSource + ".sql";
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            cmd.CommandText = "DROP DATABASE IF EXISTS " + DBTarget + "; Create database " + DBTarget;
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            constring = "server=" + instansce.Server_Name + ";port=" + instansce.Port + ";user=" + instansce.User + ";pwd=" + instansce.Pass + ";database=" + DBTarget + ";";

                            conn.ConnectionString = (constring);

                            conn.Open();
                            mb.ImportFromFile(file);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception op)
            {
                return new Exception(op.Message, op.InnerException);
            }
            return null;

        }
    }
}
