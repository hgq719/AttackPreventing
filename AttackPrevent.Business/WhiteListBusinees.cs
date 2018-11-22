using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface IWhiteListBusinees
    {
        dynamic GetWhiteListModelList(string zoneId, string authEmail, string authKey, int limit, int offset, string ip, DateTime start, DateTime end, string notes);
        bool CreateAccessRule(string zoneId, string authEmail, string authKey, string ip, string comment);
        bool DeleteAccessRule(string zoneId, string authEmail, string authKey, string ip);
    }
    public class WhiteListBusinees : IWhiteListBusinees
    {
        ICloundFlareApiService cloundFlareApiService;
        public WhiteListBusinees()
        {
            cloundFlareApiService = new CloundFlareApiService();
        }

        public bool CreateAccessRule(string zoneId, string authEmail, string authKey, string ip, string comment)
        {
            FirewallAccessRuleResponse response = cloundFlareApiService.CreateAccessRule(zoneId, authEmail, authKey, new FirewallAccessRuleRequest
            {
                configuration = new Configuration
                {
                    target = "IP",
                    value = ip,
                },
                mode = EnumMode.whitelist,
                notes = comment,
            });
            return response.success;
        }

        public bool DeleteAccessRule(string zoneId, string authEmail, string authKey, string ip)
        {
            var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, ip, "");
            var rule = list.FirstOrDefault();
            if (rule != null)
            {
                return cloundFlareApiService.DeleteAccessRule(zoneId, authEmail, authKey, rule.id).success;
            }
            return false;
        }

        public dynamic GetWhiteListModelList(string zoneId, string authEmail, string authKey, int limit, int offset, string ip, DateTime start, DateTime end, string notes)
        {
            string key = string.Format("GetWhiteListModelList:{0}-{1}-{2}", zoneId, authEmail, authKey);
            var query = Utils.GetMemoryCache<List<WhiteListModel>>(key, () =>
            {
                var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, EnumMode.whitelist);
                return list.Select(a => new WhiteListModel
                {
                    IP = a.configurationValue,
                    CreateTime = a.createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Notes = a.notes,
                }).ToList();
            }).AsQueryable();

            if (!string.IsNullOrEmpty(notes))
            {
                query = query.Where(a => a.Notes.Contains(notes));
            }
            if (!string.IsNullOrEmpty(ip))
            {
                query = query.Where(a => a.IP == ip);
            }
            if (Convert.ToDateTime(end.ToString("yyyy-MM-dd HH:mm")) > Convert.ToDateTime(start.ToString("yyyy-MM-dd HH:mm")))
            {
                if (start != DateTime.MinValue)
                {
                    query = query.Where(a => Convert.ToDateTime( a.CreateTime)>= start);
                }
                if (end != DateTime.MinValue)
                {
                    query = query.Where(a => Convert.ToDateTime(a.CreateTime) < end.AddDays(1));
                }
            }

            int total = query.Count();
            var rows = query.Skip(offset).Take(limit).ToList();
            return new { total = total, rows = rows };
        }
    }
}
