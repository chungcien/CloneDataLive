using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WS_CloneDataLive;

public class Info_MySQL_DB
{
    public string DBName { set; get; }
    public string Time_Running { set; get; }
    public List<string> Email { set; get; }

    public Exception Excute_Backup_DB(Info_MySQL_Instance SV)
    {
        try
        {
            string constring = "server=" + SV.Server_Name + ";port=" + SV.Port + ";user=" + SV.User + ";pwd=" + SV.Pass + ";database=" + DBName + ";";

            File_Read_Write.Create_Exits_Folder(AppDomain.CurrentDomain.BaseDirectory + @"MySQLBackup\" + DBName);

            string file = AppDomain.CurrentDomain.BaseDirectory + @"MySQLBackup\" + DBName + @"\" + DBName + ".sql";

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
        catch(Exception op)
        {
            return op;
        }
        return null;
        
    }
}