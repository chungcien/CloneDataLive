using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_MySQLJob
    {
        public Info_MySQLDB DBSource { set; get; }
        public List<Info_MySQLDB> Lis_DB_Target { set; get; }
        public string DayOfWeed_Running { set; get; }
        public string Time_Running { set; get; }
    }
}
