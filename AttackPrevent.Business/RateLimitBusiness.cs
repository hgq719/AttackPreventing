using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class RateLimitBusiness
    {
        public static dynamic GetAuditLog(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string url)
        {
            List<RateLimitEntity> list = new List<RateLimitEntity>();
            for (int i = 0; i < 50; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                RateLimitEntity en = new RateLimitEntity();
                en.ID = i;
                en.Period = r.Next(0, 100);
                en.Threshold = r.Next(0, 100);
                en.Url = "www.comm100.com";
                en.OrderNo = i + 1;
                en.RateLimitTriggerIpCount = r.Next(0, 100);
                en.EnlargementFactor = r.Next(0, 100);

                list.Add(en);
            }
            //List<RateLimitEntity> list = RateLimitAccess.GetRateLimits(zoneID, startTime, endTime, url);

            var total = list.Count;
            var rows = list.Skip(offset).Take(limit).ToList();

            return new { total, rows };
        }
    }
}
