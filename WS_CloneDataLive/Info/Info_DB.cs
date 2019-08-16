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

    public Exception Download_BackupFile(Info_FTPServer fTPServer)
    {
        try
        {
            bool isOK = FTPClient.Download(fTPServer.URL + "BackupDB_zip/" + ServerSource + "/" + DateTime.Now.ToString("yyyy-MM-dd"), fTPServer.User, fTPServer.Pass, AppDomain.CurrentDomain.BaseDirectory + @"Download\", DBSource + ".zip");

            if(isOK != true)
            {
                return new Exception("Download Backup file failed!", new Exception(""));
            }
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + DBSource + ".bak"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + DBSource + ".bak");
            }

            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Download\" + DBSource + ".zip", AppDomain.CurrentDomain.BaseDirectory);

        }
        catch (Exception op)
        {
            return new Exception(op.Message, op.InnerException);
        }
        return null;

    }

    public Exception Excute_Restore_DB(Info_Server info_Server)
    { 
        try
        {
            Database_Operation.RestoreDatabase(info_Server, DBTarget, AppDomain.CurrentDomain.BaseDirectory + DBSource + ".bak");

        }
        catch (Exception op)
        {
            return new Exception(op.Message, op.InnerException);
        }
        return null;

    }
}