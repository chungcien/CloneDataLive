using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_MySQL
    {
        public Info_MySQL_Instansce Instances { set; get; }
        public List<Info_MySQL_Jobs> ListJob { set; get; }
    }
   
}
