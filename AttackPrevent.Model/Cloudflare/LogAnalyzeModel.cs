using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model.Cloudflare
{
    public class LogAnalyzeModel
    {
        public string IP { get; set; }
        public string RequestHost { get; set; }
        public string RequestUrl { get; set; }
        public string RequestFullUrl { get; set; }
        public int RequestCount { get; set; }
        public int RateLimitId { get; set; }
        public int RateLimitTriggerIpCount { get; set; }
    }
}
