using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttackPrevent.Models
{
    public class RateLimitModel
    {
        public int TableID { get; set; }

        public string ZoneId { get; set; }

        public int OrderNo { get; set; }

        public string Url { get; set; }

        public int Threshold { get; set; }

        public int Period { get; set; }
        
        public int EnlargementFactor { get; set; }
        
        public int RateLimitTriggerIpCount { get; set; }

        public int RateLimitTriggerTime { get; set; }
    }
}