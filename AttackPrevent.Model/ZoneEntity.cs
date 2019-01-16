using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class ZoneEntity
    {
        public ZoneEntity()
        {
            ZoneId = string.Empty;
            ZoneName = string.Empty;
            AuthEmail = string.Empty;
            AuthKey = string.Empty;
            HostNames = string.Empty;
        }
        public int ID { get; set; }
        public int TableID { get; set; }
        public string ZoneId { get; set; }

        public string ZoneName { get; set; }

        public string AuthEmail { get; set; }

        public string AuthKey { get; set; }

        public bool IfTestStage { get; set; }

        public bool IfEnable { get; set; }

        public bool IfAttacking { get; set; }
        public int ThresholdForHost { get; set; }
        public int PeriodForHost { get; set; }

        public bool IfAnalyzeByHostRule { get; set; }
        public string HostNames { get; set; }
    }
}
