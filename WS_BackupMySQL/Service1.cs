using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WS_CloneDataLive;

namespace WS_BackupMySQL
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
        public void Start()
        {
            OnStart(new string[0]);
        }


        System.Timers.Timer timer = null;

        List<string> List_ScanAt = new List<string>();

        Info_Server _Server = new Info_Server();

        Info_FTPServer fTPServer = new Info_FTPServer();

        List<Info_DB> info_DB;

        List<Info_MySQLJob> info_JobMySQL;


        private void timer_Tick(object sender, EventArgs e)
        {

            try
            {
                info_JobMySQL = XML_read_write.ConvertXmlStringtoObject<List<Info_MySQLJob>>(System.IO.File.ReadAllText("MySQLDatabase_Config.xml"));

                for (int i = 0; i < info_JobMySQL.Count; i++)
                {
                    Info_MySQL_Instance instance = info_JobMySQL[i].Instances;
                    for (int j = 0; j< info_JobMySQL[i].ListDB.Count; j++)
                    {
                        string date_time = DateTime.Now.ToString("HH:mm");

                        if (date_time == info_JobMySQL[i].ListDB[j].Time_Running)
                        {
                            Info_DB inf = info_JobMySQL[i].ListDB[j];
                            new Thread(() => DB_Backup_Running(inf, instance)).Start();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": Error - " + ex.Message, true);
            }
        }



        protected override void OnStart(string[] args)
        {
            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), "-----------------------------------------------------", true);
            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString() + ": Service Starting...", true);

            info_DB = new List<Info_DB>();
            info_JobMySQL = new List<Info_MySQLJob>();



            //fTPServer.URL = ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString();
            //fTPServer.User = ConfigurationManager.AppSettings["FTP_USER"].ToString();
            //fTPServer.Pass = ConfigurationManager.AppSettings["FTP_PASSWORD"].ToString();
            //Info_DB inf = info_JobMySQL[1].ListDB[0];
            //DB_Backup_Running(inf, info_JobMySQL[1].Instances);
            timer.Start();
        }


        static void DB_Backup_Running(Info_DB new_DB, Info_MySQL_Instance instansce)
        {
            Compression_File Comp = new Compression_File();

            string Mess = "";
            Exception er = null;    //gán cờ er bằng null chỉ thị ko có lỗi

            try         // bắt đầu quá trình backup
            {

                //phương thức trả về 1 exception (er == null thì ko có lỗi), nếu có lỗi thì ném ra exception và dừng công việc backup
                er = new_DB.Excute_Backup_DB(instansce);
                if (er != null)
                {
                    throw new Exception("Error Backup Database", er);
                }
                else
                {
                    try
                    {
                        File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": Backup " + instansce.Server_Name + @" - " + new_DB.DBName + " Successfully!", true);
                    }
                    catch { }
                }


                // đóng nén file
                er = Comp.Run_DongNen_DB(new_DB.DBName);
                if (er != null)
                {
                    throw new Exception("Error Compression file", er);
                }
                else
                {
                    try
                    {
                        File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": Compression " + instansce.Server_Name + @" - " + new_DB.DBName + ".zip Successfully!", true);
                    }
                    catch { }
                }

                // tạo folder trong FTP server



                string path_save_inFTP = "BackupDB_zip/" + instansce.DisplayName + "/" + DateTime.Now.ToString("yyyy-MM-dd");
                er = FTPClient.CreateFolder(ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString(), ConfigurationManager.AppSettings["FTP_USER"].ToString(), ConfigurationManager.AppSettings["FTP_PASSWORD"].ToString(), path_save_inFTP);
                if (er != null)
                {
                    throw new Exception("Error Create folder FTP Server", er);
                }
                //else
                //{
                //    try
                //    {
                //        File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": Create folder " +path_save_inFTP+ " in " + ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString() + " Successfully!", true);
                //    }
                //    catch { }
                //}



                // upload file
                string file = AppDomain.CurrentDomain.BaseDirectory + @"Backup_zip\" + new_DB.DBName + ".zip";

                er = FTPClient.Upload(ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString() + path_save_inFTP, ConfigurationManager.AppSettings["FTP_USER"].ToString(), ConfigurationManager.AppSettings["FTP_PASSWORD"].ToString(), file);
                if (er != null)
                {
                    throw new Exception("Error Upload file to FTP Server", er);
                }
                else
                {
                    try
                    {
                        File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": Upload " + instansce.Server_Name + @" - " + new_DB.DBName + ".zip to " + ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString() + path_save_inFTP + " Successfully!", true);
                    }
                    catch { }
                }

                //gửi mail
                SendEmail.Send_Email(new_DB.Email, null, "[AMS - TMS] Backup Database succufully!", "Backup Database " + instansce.Server_Name + @" - " + new_DB.DBName + " succufully!", false);


                //quá trình hoàn tất 
            }
            catch (Exception io)
            {
                try
                {
                    if (io.InnerException.Message == "" || io.InnerException.Message == null)
                    {
                        Mess = io.Message;
                    }
                    else
                    {
                        Mess = io.Message + ": " + io.InnerException.Message;
                    }
                }
                catch { }

                //gửi mail
                SendEmail.Send_Email(new_DB.Email, null, "[AMS - TMS] Backup Database error!", "Server: " + instansce.Server_Name + @"\nDatabase name: " + new_DB.DBName + "\n" + Mess, false);


                File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": ----------------------------------------- ", true);
                File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": Error - " + new_DB.DBName + ": " + Mess, true);
                File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt", DateTime.Now + ": ----------------------------------------- ", true);

            }
        }


        

        protected override void OnStop()
        {
        }
    }
}
