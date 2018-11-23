using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class GlobalConfigBusiness
    {
        public static GlobalConfiguration Get()
        {
            return new GlobalConfiguration()
            {
                EmailAddForWhiteList = "elei.xu@comm100.com;summer.shen@comm100.com",
                CancelBanIPTime = 60,
                GlobalThreshold = 200,
                GlobalPeriod = 60,
                GlobalSample = 1
            };
        }
    }
}
