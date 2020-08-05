using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class RateLimitEntity
    {
        public RateLimitEntity()
        {
            OrderNo = 0;
        }
        public int ID { get; set; }

        public int TableID { get; set; }

        public string ZoneId { get; set; }
        
        public int OrderNo { get; set; }

        public string Url { get; set; }

        public int Threshold { get; set; }

        public int Period { get; set; }

        public string Action { get; set; }

        public float EnlargementFactor { get; set; }

        public DateTime LatestTriggerTime { get; set; }

        public int RateLimitTriggerIpCount { get; set; }

        public int RateLimitTriggerTime { get; set; }

        public string CreatedBy { get; set; }

        public bool IfTesting { get; set; }

        public bool IfOpenRateLimitRule { get; set; }

        public bool IfBanIp { get; set; }
    }
}
