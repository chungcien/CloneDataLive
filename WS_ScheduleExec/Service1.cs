using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WS_CloneDataLive;
using System.Configuration;

namespace WS_ScheduleExec
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

        Info_FTPServer fTPServer = new Info_FTPServer();

        List<Info_MySQL> info_MySQL;
        public void Start()
        {
            OnStart(new string[0]);
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            string date_time = DateTime.Now.ToString("dd-MM-yyyy HH:mm");

            for (int i = 0; i < info_MySQL.Count; i++)
            {
                for (int k = 0; k < info_MySQL[i].ListJob.Count; k++)
                {
                    if (date_time == info_MySQL[i].ListJob[k].Time_Running)
                    {
                        Info_MySQL_Jobs infoJob = info_MySQL[i].ListJob[k];
                        Info_MySQL_Instansce instansce = info_MySQL[i].Instances;

                        File_Read_Write.Write_File(DateTime.Now + ": Start Execute - " + infoJob.Files.Count + " file(s): " + string.Join("," ,infoJob.Files) + " on " + instansce.DB_Name + @"\" + instansce.DB_Name, true);

                        new Thread(() => MySQL_Running(instansce, infoJob)).Start();
                    }
                }

            }
        }


        protected override void OnStart(string[] args)
        {
            //Backup();
            File_Read_Write.Write_File("-----------------------------------------------------", true);
            File_Read_Write.Write_File(DateTime.Now.ToString() + ": Service Starting...", true);

            try
            {
                info_MySQL = new List<Info_MySQL>();

                info_MySQL = XML_read_write.ConvertXmlStringtoObject<List<Info_MySQL>>(System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MySQLDatabase_Config.xml"));

                File_Read_Write.Write_File(DateTime.Now + ": Load Config...Done!", true);

                File_Read_Write.Write_File(DateTime.Now + ": Load " + info_MySQL.Count + " MySQL instance(s), " + info_MySQL.Sum(i => i.ListJob.Count) + " job(s), " + info_MySQL.Sum(i => i.ListJob.Sum(k => k.Files.Count)) + " file(s).", true);

                timer.Start();

            }
            catch
            {
                File_Read_Write.Write_File(DateTime.Now + ": Load Config...failed!", true);
            }



            //Info_MySQL_Jobs infoJob = info_MySQL[0].ListJob[0];
            //Info_MySQL_Instansce instansce = info_MySQL[0].Instances;

            //MySQL_Running(instansce, infoJob);

        }

        bool MySQL_Running(Info_MySQL_Instansce instansce, Info_MySQL_Jobs job)
        {

            string Mess = "";
            Exception er = null;    //gán cờ er bằng null chỉ thị ko có lỗi
            List<string> logs = new List<string>();
            Thread.CurrentThread.IsBackground = true;

            er = job.Execute_DB(instansce);
            if (er != null)
            {
                Mess = er.Message + ": " + ((er.InnerException == null) ? "" : er.InnerException.Message);

                logs.Add(DateTime.Now + ": Error - " + instansce.DB_Name + ": " + Mess);
                File_Read_Write.Write_File(DateTime.Now + ": Error - " + instansce.DB_Name + ": " + Mess, true);

                SendEmail.Send_Email(instansce.To_Email, null, "Execute SQL File(s) error!", "Server: " + instansce.Server_Name + @"\nDatabase name: " + instansce.DB_Name + "\n" + Mess + "\nLog:\n" + string.Join(Environment.NewLine, logs), false);

                return false;
            }
            else
            {
                logs.Add(DateTime.Now + ": Execute SQL File(s) " + instansce.Server_Name + @"\" + instansce.DB_Name + " done!");
                File_Read_Write.Write_File(DateTime.Now + ": Execute SQL File(s) " + instansce.Server_Name + @"\" + instansce.DB_Name + " done!", true);
            }

            try
            {
                string t = new SSHClient().SSH_Exec("cd /root/Audition/ && ./client_master restoreMetaCache");

                logs.Add(DateTime.Now + ": Restore metaCache succufully! Result: " + t);
                File_Read_Write.Write_File(DateTime.Now + ": Restore metaCache succufully! Result: " + t, true);
            }
            catch(Exception io)
            {
                logs.Add(DateTime.Now + ": Error - " + io.Message);
                File_Read_Write.Write_File(DateTime.Now + ": Error - " + io.Message, true);

                return false;
            }

            SendEmail.Send_Email(instansce.To_Email, null, "Execute SQL File(s) succufully!", "Applied SQL File(s) to " + instansce.Server_Name + @"\" + instansce.DB_Name + " succufully!\n" + "\nLog:\n" + string.Join(Environment.NewLine, logs), false);

            logs.Clear();

            return true;
        }




        protected override void OnStop()
        {
            timer.Stop();

            File_Read_Write.Write_File("-----------------------------------------------------", true);

            File_Read_Write.Write_File(DateTime.Now.ToString() + ": Service stopped!", true);

            File_Read_Write.Write_File("-----------------------------------------------------", true);

        }
    }
}
