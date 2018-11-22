using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class GetCloundflareLogsBackgroundInfo
    {
        public string Guid { get; set; }
        public string ZoneId { get; set; }
        public string AuthEmail { get; set; }
        public string AuthKey { get; set; }
        public double Sample { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EnumBackgroundStatus Status { get; set; }
        public List<CloudflareLog> CloudflareLogs { get; set; }
}
    public enum EnumBackgroundStatus
    {
        Processing,
        Succeeded,
        Failed
    }
}
