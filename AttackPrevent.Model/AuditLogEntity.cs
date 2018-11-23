using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class AuditLogEntity
    {
        public int ID { get; set; }
        public string ZoneID { get; set; }
        public string LogType { get; set; }
        public DateTime LogTime { get; set; }
        public string LogOperator { get; set; }
        public string Detail { get; set; }

        public string IP { get; set; }
    }
}
