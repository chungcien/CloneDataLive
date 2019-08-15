using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using WS_CloneDataLive;

public class Info_DB
{
    public string ServerSource { set; get; }
    public string DBSource { set; get; }
    public string DBTarget { set; get; }
    public string DayOfWeed_Running { set; get; }
    public string Time_Running { set; get; }
    public List<string> Email { set; get; }
    public Info_DB()
    {

    }

    public Exception Excute_Restore_DB(Info_Server info_Server, Info_FTPServer fTPServer)
    { 
        try
        {
            FTPClient.Download(fTPServer.URL + "BackupDB_zip/" + ServerSource + "/" + DateTime.Now.ToString("yyyy-MM-dd"), fTPServer.User, fTPServer.Pass, AppDomain.CurrentDomain.BaseDirectory + @"Download\", DBSource + ".zip");

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + DBSource + ".bak"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + DBSource + ".bak");
            }

            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Download\" + DBSource + ".zip", AppDomain.CurrentDomain.BaseDirectory);


            Database_Operation.RestoreDatabase(info_Server, DBTarget, AppDomain.CurrentDomain.BaseDirectory + DBSource + ".bak");

        }
        catch (Exception op)
        {
            return op;
        }
        return null;

    }
}