using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{

    public class SmoOperations
    {

        public SmoOperations()
        {
            
        }
        

        public void Backup(string FileName,string dbName, string[] Tables)
        {
            StringBuilder sb = new StringBuilder();
            Server srv = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection("localhost", "sa", "123"));
            Database dbs = srv.Databases[dbName];
            ScriptingOptions options = new ScriptingOptions();
            options.ScriptData = true;
            options.ScriptDrops = false;
            options.FileName = FileName;
            options.EnforceScriptingOptions = true;
            options.ScriptSchema = false;
            options.IncludeHeaders = true;
            options.AppendToFile = true;
            options.Indexes = true;
            options.WithDependencies = true;
            foreach (var tbl in Tables)
            {
                dbs.Tables[tbl].EnumScript(options);
            }
        }

        //public void GetTransferScript(string FileName, Info_Instances db)
        //{

        //    Server srv = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection(db.Server_Name, db.User, db.Pass));
        //    Database dbs = srv.Databases[db.DBName];

        //    var transfer = new Transfer(dbs);

        //    transfer.CopyAllObjects = true;
        //    transfer.CopyAllSynonyms = true;
        //    transfer.CopyData = false;

        //    // additional options
        //    //transfer.Options.ScriptDrops = true;
        //    transfer.Options.WithDependencies = true;
        //    transfer.Options.DriAll = true;
        //    transfer.Options.Triggers = true;
        //    transfer.Options.Indexes = true;
        //    transfer.Options.SchemaQualifyForeignKeysReferences = true;
        //    transfer.Options.ExtendedProperties = true;
        //    transfer.Options.IncludeDatabaseRoleMemberships = true;
        //    transfer.Options.Permissions = true;
        //    transfer.PreserveDbo = true;
        //    transfer.Options.ScriptData = true;
        //    transfer.Options.IncludeHeaders = true;
        //    transfer.Options.AppendToFile = false;
        //    transfer.Options.FileName = FileName;

        //    // generates script
        //    transfer.EnumScriptTransfer();
        //}

        //public void ExcuteSQLScript(string FileName, List<Info_Instances> db, bool DropDB)
        //{
        //    for(int i = 0; i< db.Count; i++)
        //    {

        //        Server srv = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection(db[i].Server_Name, db[i].User, db[i].Pass));
        //        Database dbs = srv.Databases[db[i].DBName];

        //        if(DropDB == true)
        //        {
        //            dbs.ExecuteNonQuery("USE Master; alter database [" + db[i].DBName + "] set single_user with rollback immediate; IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'"+ db[i].DBName + "') DROP DATABASE["+ db[i].DBName + "]; Create database "+ db[i].DBName + "; Use " + db[i].DBName  + "; " + System.IO.File.ReadAllText(FileName));
        //        }
        //        else
        //        {
        //            dbs.ExecuteNonQuery(System.IO.File.ReadAllText(FileName));

        //        }
        //    }
        //}
    }


}
