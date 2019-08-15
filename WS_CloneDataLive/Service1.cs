using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace WS_CloneDataLive
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();

            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Tick);
            timer.Interval = 60000;
        }

        System.Timers.Timer timer = null;

        List<string> List_ScanAt = new List<string>();

        Info_Server _Server = new Info_Server();

        Info_FTPServer fTPServer = new Info_FTPServer();

        List<Info_DB> info_DB;

        List<Info_MySQLJob> info_JobMySQL;
        public void Start()
        {
            OnStart(new string[0]);
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            string date_time = DateTime.Now.ToString("HH:mm");
            for (int i = 0; i < info_DB.Count; i++)
            {
                if (date_time == info_DB[i].Time_Running)
                {
                    List<string> day_running = info_DB[i].DayOfWeed_Running.Split(',').ToList();

                    for (int j = 0; j < day_running.Count; j++)
                    {
                        if (DateTime.Now.DayOfWeek.ToString().ToLower() == day_running[j].ToLower())
                        {
                            new Thread(() => SQLServer_Running(info_DB[i])).Start();
                        }
                    }
                }
            }

        }


        protected override void OnStart(string[] args)
        {
            //Restore();
            //Backup();
            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), "-----------------------------------------------------", true);
            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString() + ": Service Starting...", true);

            info_DB = new List<Info_DB>();
            info_JobMySQL = new List<Info_MySQLJob>();

            info_DB = XML_read_write.ConvertXmlStringtoObject<List<Info_DB>>(System.IO.File.ReadAllText("Database_Config.xml"));

            info_JobMySQL = XML_read_write.ConvertXmlStringtoObject<List<Info_MySQLJob>>(System.IO.File.ReadAllText("MySQLDatabase_Config.xml"));


            fTPServer.URL = ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString();
            fTPServer.User = ConfigurationManager.AppSettings["FTP_USER"].ToString();
            fTPServer.Pass = ConfigurationManager.AppSettings["FTP_PASSWORD"].ToString();

            _Server.ServerName = ConfigurationManager.AppSettings["Server_Name"].ToString();
            _Server.User = ConfigurationManager.AppSettings["User"].ToString();
            _Server.Pass = ConfigurationManager.AppSettings["Pass"].ToString();

            //Running(list_job[0]);

            //SQLServer_Running(info_DB[0]);

            for(int i = 0; i< info_JobMySQL.Count; i++)
            {
                Backup(info_JobMySQL[i].DBSource);
            }

        }


        void SQLServer_Running(Info_DB Job)
        {
            Thread.CurrentThread.IsBackground = true;


            FTPClient.Download(fTPServer.URL + "BackupDB_zip/" + Job.ServerSource + "/" + DateTime.Now.ToString("yyyy-MM-dd"), fTPServer.User, fTPServer.Pass, AppDomain.CurrentDomain.BaseDirectory + @"Download\", Job.DBSource + ".zip");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Job.DBSource + ".bak"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + Job.DBSource + ".bak");
            }

            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Download\" + Job.DBSource + ".zip", AppDomain.CurrentDomain.BaseDirectory);


            Database_Operation.RestoreDatabase(_Server.ServerName, Job.DBTarget, AppDomain.CurrentDomain.BaseDirectory + Job.DBSource + ".bak");


            //var ops = new SmoOperations();
            // var temp = ops.TableNames(Job.DB_Source.DBName);

            // ops.GetTransferScript(AppDomain.CurrentDomain.BaseDirectory + "test.sql", Job.DB_Source);

            //ops.ExcuteSQLScript(AppDomain.CurrentDomain.BaseDirectory + "test.sql", Job.Lis_DB_Target, true);

        }

        void MySQL_Running(Info_MySQLJob Job)
        {
            Thread.CurrentThread.IsBackground = true;


            FTPClient.Download(fTPServer.URL + "BackupDB_zip/" + Job.DBSource.DBName + "/" + DateTime.Now.ToString("yyyy-MM-dd"), fTPServer.User, fTPServer.Pass, AppDomain.CurrentDomain.BaseDirectory + @"Download\", Job.DBSource.DBName + ".zip");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Job.DBSource.DBName + ".sql"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + Job.DBSource.DBName + ".sql");
            }

            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Download\" + Job.DBSource.DBName + ".zip", AppDomain.CurrentDomain.BaseDirectory);

        }

        private void Backup(Info_MySQLDB DB)
        {
            string constring = "server=" + DB.Server_Name + ";port=" + DB.Port + ";user=" + DB.User + ";pwd=" + DB.Pass + ";database=" + DB.DBName + ";";

            File_Read_Write.Create_Exits_Folder(AppDomain.CurrentDomain.BaseDirectory + @"MySQLBackup\");

            string file = AppDomain.CurrentDomain.BaseDirectory + @"MySQLBackup\" + DB.DBName + ".sql";

            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file);
                        conn.Close();
                    }
                }
            }
        }

        private void Restore(Info_MySQLDB DB)
        {
            string constring = "server=" + DB.Server_Name + ";port=" + DB.Port + ";user=" + DB.User + ";pwd=" + DB.Pass + ";database=" + DB.DBName + ";";
            string file = AppDomain.CurrentDomain.BaseDirectory + DB.DBName + ".sql";
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

        protected override void OnStop()
        {
            timer.Stop();

            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), "-----------------------------------------------------", true);

            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString() + ": Service stopped!", true);

            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), "-----------------------------------------------------", true);

        }
    }
}
