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
        public static dynamic GetRateLimit(int limit, int offset, string zoneID, DateTime? startTime, DateTime? endTime, string url)
        {
            //List<RateLimitEntity> list = new List<RateLimitEntity>();
            //for (int i = 0; i < 50; i++)
            //{
            //    var r = new Random(Guid.NewGuid().GetHashCode());
            //    RateLimitEntity en = new RateLimitEntity();
            //    en.ID = i;
            //    en.Period = r.Next(0, 100);
            //    en.Threshold = r.Next(0, 100);
            //    en.Url = "www.comm100.com";
            //    en.OrderNo = i + 1;
            //    en.RateLimitTriggerIpCount = r.Next(0, 100);
            //    en.EnlargementFactor = r.Next(0, 100);

            //    list.Add(en);
            //}
            List<RateLimitEntity> list = RateLimitAccess.GetRateLimits(zoneID, startTime, endTime, url);

            var total = list.Count;
            var rows = list.Skip(offset).Take(limit).ToList();

            return new { total, rows };
        }

        public static List<RateLimitEntity> GetList(string zoneId)
        {
            return RateLimitAccess.GetRateLimits(zoneId, null, null, null);
        }

        public static void Add(RateLimitEntity item)
        {
            var list = RateLimitAccess.GetRateLimits(item.ZoneId, null, null, string.Empty);
            var orderMax = 0;
            if (null != list.LastOrDefault())
            {
                orderMax = list.LastOrDefault().OrderNo;
            }
            item.OrderNo = orderMax + 1;
            RateLimitAccess.Add(item);
        }

        public static RateLimitEntity GetRateLimitByID(int id)
        {
            return RateLimitAccess.GetRateLimitByID(id);
        }

        public static void Update(RateLimitEntity item)
        {
            RateLimitEntity rateLimitOld = RateLimitAccess.GetRateLimitByID(item.TableID);
            if (item.ZoneId == rateLimitOld.ZoneId)
            {
                RateLimitAccess.Edit(item);
            }
            else
            {
                Delete(rateLimitOld.TableID, rateLimitOld.OrderNo, rateLimitOld.ZoneId);
                RateLimitBusiness.Add(item);
            }
        }

        public static void TriggerRateLimit(RateLimitEntity rateLimit)
        {
            RateLimitAccess.TriggerRateLimit(rateLimit);
        }

        public static void Delete(int id, int order, string zoneId)
        {
            RateLimitAccess.Delete(id, order, zoneId);
        }

        public static void UpdateOrder(int action, int id, int order, string zoneId)
        {
            int toOrder = action == 1 ? order - 1 : order + 1;
            RateLimitEntity toItem = RateLimitAccess.GetRateLimitByOrderNo(toOrder, zoneId);
            if (toItem.TableID > 0)
            {
                RateLimitAccess.EditOrder(id, toOrder, toItem.TableID, order);
            }
        }

        public static int GetRateLimitMaxOrder(string zoneId)
        {
            return RateLimitAccess.GetRateLimitMaxOrder(zoneId);
        }
    }
}
