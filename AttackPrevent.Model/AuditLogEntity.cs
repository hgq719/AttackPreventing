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

        public AuditLogEntity() { }

        public AuditLogEntity(string zoneId, LogLevel logType, string detail)
        {
            ZoneID = zoneId;
            LogType = logType.ToString();
            LogTime = DateTime.Now;
            Detail = string.Format("[{0}] {1} {2}", LogType, LogTime.ToString("yyyy-MM-dd HH:mm:ss fff"), detail);
            LogOperator = "System";
        }
    }

    public enum LogLevel
    {
        App,
        Error,
        Audit
    }
}
