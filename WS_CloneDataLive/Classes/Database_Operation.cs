﻿using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    class Database_Operation
    {
        static Server srv;
        static ServerConnection conn;

        public static Exception RestoreDatabase(Info_Server info_Server, string databaseName, string filePath)
        {

            conn = new ServerConnection(info_Server.Server_Name, info_Server.User, info_Server.Pass);
            //conn.ServerInstance = serverName;
            srv = new Server(conn);

            try
            {
                Restore res = new Restore();

                res.Devices.AddDevice(filePath, DeviceType.File);

                RelocateFile DataFile = new RelocateFile();
                string MDF = res.ReadFileList(srv).Rows[0][1].ToString();
                DataFile.LogicalFileName = res.ReadFileList(srv).Rows[0][0].ToString();
                DataFile.PhysicalFileName = srv.Databases[databaseName].FileGroups[0].Files[0].FileName;

                RelocateFile LogFile = new RelocateFile();
                string LDF = res.ReadFileList(srv).Rows[1][1].ToString();
                LogFile.LogicalFileName = res.ReadFileList(srv).Rows[1][0].ToString();
                LogFile.PhysicalFileName = srv.Databases[databaseName].LogFiles[0].FileName;

                res.RelocateFiles.Add(DataFile);
                res.RelocateFiles.Add(LogFile);

                res.Database = databaseName;
                res.NoRecovery = false;
                res.ReplaceDatabase = true;
                res.SqlRestore(srv);
                conn.Disconnect();
            }
            catch (SmoException ex)
            {
                return new Exception(ex.Message, ex.InnerException);
            }
            catch (IOException ex)
            {
                return new Exception(ex.Message, ex.InnerException);
            }

            return null;
        }

        public static Server Getdatabases(string serverName)
        {
            conn = new ServerConnection();
            conn.ServerInstance = serverName;

            srv = new Server(conn);
            conn.Disconnect();
            return srv;

        }
    }
}
