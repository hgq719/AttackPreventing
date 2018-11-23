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
                    target = "ip",
                    value = ip,
                },
                mode = EnumMode.whitelist,
                notes = comment,
            });
            if (response.success)
            {
                string key = string.Format("GetWhiteListModelList:{0}-{1}-{2}", zoneId, authEmail, authKey);
                Utils.RemoveMemoryCache(key);
            }
            return response.success;
        }

        public bool DeleteAccessRule(string zoneId, string authEmail, string authKey, string ip)
        {
            var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, ip, "");
            var rule = list.FirstOrDefault();
            if (rule != null)
            {
                FirewallAccessRuleResponse response = cloundFlareApiService.DeleteAccessRule(zoneId, authEmail, authKey, rule.id);
                if (response.success)
                {
                    string key = string.Format("GetWhiteListModelList:{0}-{1}-{2}", zoneId, authEmail, authKey);
                    Utils.RemoveMemoryCache(key);
                }
                return response.success;
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
            }, 5).AsQueryable();

            //var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, EnumMode.whitelist);
            //var query = list.Select(a => new WhiteListModel
            //{
            //    IP = a.configurationValue,
            //    CreateTime = a.createTime.ToString("yyyy-MM-dd HH:mm:ss"),
            //    Notes = a.notes,
            //}).AsQueryable();

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
