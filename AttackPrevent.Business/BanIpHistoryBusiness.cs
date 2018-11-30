using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class BanIpHistoryBusiness
    {
        public static List<BanIpHistory> Get(string zoneId, string ip = null)
        {
            try
            {
                return BanIpHistoryAccess.Get(zoneId, ip);
            }
            catch (Exception ex)
            {
                AuditLogBusiness.Add(new AuditLogEntity(zoneId, LogLevel.Error,
                    $"Get ban ip histories failure, the reason is:[{ex.Message}].<br />stack trace:{ex.StackTrace}."));
                return new List<BanIpHistory>();
            }
        }

        public static bool Add(BanIpHistory banIPHistory)
        {
            try
            {
                var banIpHistories = BanIpHistoryAccess.Get(banIPHistory.ZoneId, banIPHistory.IP);
                if (null != banIpHistories && banIpHistories.Count > 0)
                {
                    return BanIpHistoryAccess.Update(banIPHistory);
                }
                else
                {
                    return BanIpHistoryAccess.Add(banIPHistory);
                }
            }
            catch (Exception ex)
            {
                AuditLogBusiness.Add(new AuditLogEntity(banIPHistory.ZoneId, LogLevel.Error,
                    $"Add ban ip histories failure, the reason is:[{ex.Message}].<br />stack trace:{ex.StackTrace}."));
                return false;
            }
        }

        public static bool Delete(string zoneId, int id)
        {
            try
            {
                return BanIpHistoryAccess.Delete(id);
            }
            catch (Exception ex)
            {
                AuditLogBusiness.Add(new AuditLogEntity(zoneId, LogLevel.Error,
                    $"delete ban ip histories failure, the reason is:[{ex.Message}].<br />stack trace:{ex.StackTrace}."));
                return false;
            }
        }

        public static void AddList(List<BanIpHistory> banIPHistory)
        {
            BanIpHistoryAccess.Add(banIPHistory);
        }
    }
}
