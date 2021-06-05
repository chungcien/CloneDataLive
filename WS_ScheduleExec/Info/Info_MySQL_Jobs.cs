using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_MySQL_Jobs
    {
        public string Time_Running { set; get; }
        public List<string> Files { set; get; }


        public Exception Execute_DB(Info_MySQL_Instansce instansce)
        {
            try
            {

                string constring = "server=" + instansce.Server_Name + ";port=" + instansce.Port + ";user=" + instansce.User + ";pwd=" + instansce.Pass + ";database=" + instansce.DB_Name + ";";

                string text = "";

                foreach(var i in Files)
                {
                    text = text + File.ReadAllText(i);
                }

                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.CommandText = text;
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception op)
            {
                return new Exception(op.Message, op.InnerException);
            }
            return null;

        }
    }
}
