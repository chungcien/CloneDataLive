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

        List<Info_Instances> info_Instances;
        public void Start()
        {
            OnStart(new string[0]);
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            string date_time = DateTime.Now.ToString("HH:mm");
            for (int i = 0; i < info_Instances.Count; i++)
            {
                if (date_time == info_Instances[i].Time_Running)
                {
                    List<string> day_running = info_Instances[i].DayOfWeed_Running.Split(',').ToList();

                    for (int j = 0; j < day_running.Count; j++)
                    {
                        if (DateTime.Now.DayOfWeek.ToString().ToLower() == day_running[j].ToLower())
                        {
                            new Thread(() => Running(info_Instances[i])).Start();
                        }
                    }
                }
            }

        }

        protected override void OnStart(string[] args)
        {

            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), "-----------------------------------------------------", true);
            File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString() + ": Service Starting...", true);

            info_Instances = new List<Info_Instances>();

            info_Instances = XML_read_write.ConvertXmlStringtoObject<List<Info_Instances>>(System.IO.File.ReadAllText("Database_Config.xml"));

            //Running(list_job[0]);

            Running(info_Instances[0]);
        }


        void Running(Info_Instances Job)
        {
            Thread.CurrentThread.IsBackground = true;


            FTPClient.Download("ftp://192.168.68.12/BackupDB_zip/192.168.67.13/" + DateTime.Now.ToString("yyyy-MM-dd"), "ftp_rrc", "LJ3(srh!,L", AppDomain.CurrentDomain.BaseDirectory, @"TMS.zip");

            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "TMS.bak"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "TMS.bak");
            }
            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "TMS.zip", AppDomain.CurrentDomain.BaseDirectory);

            for (int i = 0; i<Job.DBName.Count; i++)
            {
                Database_Operation.RestoreDatabase(Job.Server_Name, Job.DBName[i], AppDomain.CurrentDomain.BaseDirectory + Job.backupFilename);
            }



            //var ops = new SmoOperations();
            // var temp = ops.TableNames(Job.DB_Source.DBName);

            // ops.GetTransferScript(AppDomain.CurrentDomain.BaseDirectory + "test.sql", Job.DB_Source);

            //ops.ExcuteSQLScript(AppDomain.CurrentDomain.BaseDirectory + "test.sql", Job.Lis_DB_Target, true);

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
