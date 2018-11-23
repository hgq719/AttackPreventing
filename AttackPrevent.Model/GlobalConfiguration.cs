using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class GlobalConfiguration
    {
        public string EmailAddForWhiteList { get; set; }
        public int CancelBanIPTime { get; set; }
        public int GlobalThreshold { get; set; }
        public int GlobalPeriod { get; set; }
        public string ValidateCode { get; set; }
        public double GlobalSample { get; set; }
        public int GlobalTimeSpan { get; set; }
    }
}
