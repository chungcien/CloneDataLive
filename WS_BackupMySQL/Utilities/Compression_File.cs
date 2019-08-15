using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

namespace WS_CloneDataLive
{
    public class Compression_File
    {
        public Exception Run_DongNen_DB(string DBName)
        {
            try
            {
                Ionic.Zip.ZipFile file = new ZipFile();
                file.AddDirectory(AppDomain.CurrentDomain.BaseDirectory + @"MySQLBackup\" + DBName);

                //System.IO.DirectoryInfo File_Info = new System.IO.DirectoryInfo(Registry_Read_Write.Read_Value(@"SOFTWARE\MaiA Sync\" + Server_Name + @"\SaveBackupTo", DB_Name));
                File_Read_Write.Create_Exits_Folder(AppDomain.CurrentDomain.BaseDirectory + @"Backup_zip");
                // file.SaveProgress += Save_Process;
                file.Save(AppDomain.CurrentDomain.BaseDirectory + @"Backup_zip\" + DBName + ".zip");

            }
            catch (Exception er)
            {
                
                return er;
            }
            return null;
        }
    }
}
