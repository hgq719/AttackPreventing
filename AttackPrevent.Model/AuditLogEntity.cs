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
        public int ZoneTableID { get; set; }
        public LogLevel LogType { get; set; }
        public DateTime LogTime { get; set; }
        public string LogOperator { get; set; }
        public string Detail { get; set; }

        public string IP { get; set; }

        public AuditLogEntity() { }

        public AuditLogEntity(int zoneTableId, LogLevel logType, string detail)
        {
            ZoneTableID = zoneTableId;
            LogType = logType;
            LogTime = DateTime.UtcNow;
            Detail = $"[{LogType}] {LogTime:MM/dd/yyyy HH:mm:ss fff} {detail}";
            IP = "127.0.0.1";
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
