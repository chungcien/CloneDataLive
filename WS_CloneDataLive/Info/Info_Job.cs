using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_CloneDataLive
{
    public class Info_Job
    {
        public Info_Instances DB_Source { set; get; }
        public List<Info_Instances> Lis_DB_Target { set; get; }
        public string DayOfWeed_Running { set; get; }
        public string Time_Running { set; get; }

        public Info_Job()
        {

        }
        public Info_Job(Info_Instances DBSource, List<Info_Instances> lis_DBTarget)
        {
            DB_Source = DBSource;
            Lis_DB_Target = lis_DBTarget;
        }

    }
}
