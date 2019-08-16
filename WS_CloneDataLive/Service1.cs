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

        string log_path = AppDomain.CurrentDomain.BaseDirectory + @"Log\Service_Log.txt";

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
                            Info_DB infoDB = info_DB[i];
                            new Thread(() => SQLServer_Running(infoDB, _Server)).Start();
                        }
                    }
                }
            }


            for (int i = 0; i < info_JobMySQL.Count; i++)
            {
                for (int k = 0; k < info_JobMySQL[i].ListDB.Count; k++)
                {
                    if (date_time == info_JobMySQL[i].ListDB[k].Time_Running)
                    {
                        List<string> day_running = info_JobMySQL[i].ListDB[k].DayOfWeed_Running.Split(',').ToList();

                        for (int j = 0; j < day_running.Count; j++)
                        {
                            if (DateTime.Now.DayOfWeek.ToString().ToLower() == day_running[j].ToLower())
                            {
                                Info_MySQL_DB infoDB = info_JobMySQL[i].ListDB[k];
                                Info_MySQL_Instansce instansce = info_JobMySQL[i].Instances;

                                new Thread(() => MySQL_Running(instansce, infoDB)).Start();
                            }
                        }
                    }
                }

            }
        }


        protected override void OnStart(string[] args)
        {

            //Backup();
            File_Read_Write.Write_File(log_path, "-----------------------------------------------------", true);
            File_Read_Write.Write_File(log_path, DateTime.Now.ToString() + ": Service Starting...", true);

            try
            {
                info_DB = new List<Info_DB>();
                info_JobMySQL = new List<Info_MySQLJob>();

                info_DB = XML_read_write.ConvertXmlStringtoObject<List<Info_DB>>(System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Database_Config.xml"));

                info_JobMySQL = XML_read_write.ConvertXmlStringtoObject<List<Info_MySQLJob>>(System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MySQLDatabase_Config.xml"));


                fTPServer.URL = ConfigurationManager.AppSettings["FTP_SERVER_URI"].ToString();
                fTPServer.User = ConfigurationManager.AppSettings["FTP_USER"].ToString();
                fTPServer.Pass = ConfigurationManager.AppSettings["FTP_PASSWORD"].ToString();

                _Server.Server_Name = ConfigurationManager.AppSettings["Server_Name"].ToString();
                _Server.User = ConfigurationManager.AppSettings["User"].ToString();
                _Server.Pass = ConfigurationManager.AppSettings["Pass"].ToString();

                File_Read_Write.Write_File(log_path, DateTime.Now + ": Load Config...Done!", true);

                timer.Start();

            }
            catch
            {
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Load Config...failed!", true);
            }

            //Running(list_job[0]);

            //SQLServer_Running(info_DB[0]);

            //MySQL_Running(info_JobMySQL[0].Instances, info_JobMySQL[0].ListDB[0]);

            //for(int i = 0; i< info_JobMySQL.Count; i++)
            //{
            //    Restore(info_JobMySQL[i].DBSource);
            //}

        }


        bool SQLServer_Running(Info_DB DB, Info_Server instansce)
        {
            string Mess = "";
            Exception er = null;    //gán cờ er bằng null chỉ thị ko có lỗi

            Thread.CurrentThread.IsBackground = true;

            er = DB.Download_BackupFile(fTPServer);
            if (er != null)
            {
                Mess = er.Message + ": " + ((er.InnerException == null) ? "" : er.InnerException.Message);
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Error - " + DB.DBTarget + ": " + Mess, true);

                SendEmail.Send_Email(DB.Email, null, "[AMS - TMS] Clone Database error!", "Server: " + instansce.Server_Name + @"\nDatabase name: " + DB.DBTarget + "\n" + Mess, false);

                return false;
            }
            else
            {
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Download Backup file " + fTPServer.URL + "BackupDB_zip/" + DB.ServerSource + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + DB.DBSource + ".zip....done!", true);
            }


            er = DB.Excute_Restore_DB(instansce);
            if (er != null)
            {
                Mess = er.Message + ": " + ((er.InnerException == null) ? "" : er.InnerException.Message);
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Error - " + DB.DBTarget + ": " + Mess, true);
                SendEmail.Send_Email(DB.Email, null, "[AMS - TMS] Clone Database error!", "Server: " + instansce.Server_Name + @"\nDatabase name: " + DB.DBTarget + "\n" + Mess, false);

                return false;
            }
            else
            {
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Restore " + DB.ServerSource + @"\" + DB.DBSource + " to " + instansce.Server_Name + @"\" + DB.DBTarget + " done!", true);
            }

            SendEmail.Send_Email(DB.Email, null, "[AMS - TMS] Clone Database succufully!", "Clone Database " + DB.ServerSource + @"\" + DB.DBSource + " to " + instansce.Server_Name + @"\" + DB.DBTarget + " succufully!", false);

            return true;
        }

        bool MySQL_Running(Info_MySQL_Instansce instansce, Info_MySQL_DB DB)
        {

            string Mess = "";
            Exception er = null;    //gán cờ er bằng null chỉ thị ko có lỗi

            Thread.CurrentThread.IsBackground = true;

            er = DB.Download_BackupFile(fTPServer);
            if (er != null)
            {
                Mess = er.Message + ": " + ((er.InnerException == null) ? "" : er.InnerException.Message);
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Error - " + DB.DBTarget + ": " + Mess, true);

                SendEmail.Send_Email(DB.Email, null, "[Dashboard - RTS] Clone Database error!", "Server: " + instansce.Server_Name + @"\nDatabase name: " + DB.DBTarget + "\n" + Mess, false);

                return false;
            }
            else
            {
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Download Backup file " + fTPServer.URL + "BackupDB_zip/" + DB.ServerSource + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + DB.DBSource + ".zip....done!", true);
            }


            er = DB.Excute_Restore_DB(instansce);
            if (er != null)
            {
                Mess = er.Message + ": " + ((er.InnerException == null) ? "" : er.InnerException.Message);
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Error - " + DB.DBTarget + ": " + Mess, true);

                SendEmail.Send_Email(DB.Email, null, "[Dashboard - RTS] Clone Database error!", "Server: " + instansce.Server_Name + @"\nDatabase name: " + DB.DBTarget + "\n" + Mess, false);

                return false;
            }
            else
            {
                File_Read_Write.Write_File(log_path, DateTime.Now + ": Restore " + DB.ServerSource + @"\" + DB.DBSource + " to " + instansce.Server_Name + @"\" + DB.DBTarget + " done!", true);
            }

            SendEmail.Send_Email(DB.Email, null, "[Dashboard - RTS] Clone Database succufully!", "Clone Database " + DB.ServerSource + @"\" + DB.DBSource + " to " + instansce.Server_Name + @"\" + DB.DBTarget + " succufully!", false);

            return true;
        }




        protected override void OnStop()
        {
            timer.Stop();

            File_Read_Write.Write_File(log_path, "-----------------------------------------------------", true);

            File_Read_Write.Write_File(log_path, DateTime.Now.ToString() + ": Service stopped!", true);

            File_Read_Write.Write_File(log_path, "-----------------------------------------------------", true);

        }
    }
}
