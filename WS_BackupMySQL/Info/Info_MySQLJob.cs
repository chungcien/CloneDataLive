using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_MySQLJob
    {
        public Info_MySQL_Instance Instances { set; get; }
        public List<Info_MySQL_DB> ListDB { set; get; }
    }
}
