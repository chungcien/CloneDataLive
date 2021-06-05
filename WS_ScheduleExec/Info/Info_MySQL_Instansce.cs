using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_MySQL_Instansce
    {
        public string DB_Name { set; get; }
        public string Server_Name { set; get; }
        public int Port { set; get; }
        public string User { set; get; }
        public string Pass { set; get; }
        public List<string> To_Email { get; set; }
    }
}
