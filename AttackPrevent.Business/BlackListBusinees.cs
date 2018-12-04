using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AttackPrevent.Business
{
    public interface IBlackListBusinees
    {
        dynamic GetBlackListModelList(string zoneId, string authEmail, string authKey, int limit, int offset, string ip, DateTime start, DateTime end, string notes);
        bool CreateAccessRule(string zoneId, string authEmail, string authKey, string ip, string comment);
        bool DeleteAccessRule(string zoneId, string authEmail, string authKey, string ip);
    }
    public class BlackListBusinees : IBlackListBusinees
    {
        ICloundFlareApiService cloundFlareApiService;
        public BlackListBusinees()
        {
            cloundFlareApiService = new CloundFlareApiService();
        }

        public bool CreateAccessRule(string zoneId, string authEmail, string authKey, string ip, string comment)
        {
            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneId);
            FirewallAccessRuleResponse response = new FirewallAccessRuleResponse
            {
                success = true
            };
            if (zone.IfEnable && zone.IfAttacking && !zone.IfTestStage)
            {
                //Code review by michael, 这里面是可能有异常的.
                response = cloundFlareApiService.CreateAccessRule(zoneId, authEmail, authKey, new FirewallAccessRuleRequest
                {
                    configuration = new Configuration
                    {
                        target = "ip",
                        value = ip,
                    },
                    mode = EnumMode.challenge,
                    notes = comment,
                });

                if (response.success)
                {
                    string key = string.Format("GetBlackListModelList:{0}-{1}-{2}", zoneId, authEmail, authKey);
                    Utils.RemoveMemoryCache(key);
                }
            }
            return response.success;
        }

        public bool DeleteAccessRule(string zoneId, string authEmail, string authKey, string ip)
        {
            var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, ip, "");
            var rule = list.FirstOrDefault();
            FirewallAccessRuleResponse response = new FirewallAccessRuleResponse
            {
                success = true
            };
            if (rule != null)
            {
                var zoneList = ZoneBusiness.GetZoneList();
                var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneId);
                if (zone.IfEnable && zone.IfAttacking && !zone.IfTestStage)
                {
                    response = cloundFlareApiService.DeleteAccessRule(zoneId, authEmail, authKey, rule.id);
                    if (response.success)
                    {
                        string key = string.Format("GetBlackListModelList:{0}-{1}-{2}", zoneId, authEmail, authKey);
                        Utils.RemoveMemoryCache(key);
                    }
                }
                return response.success;
            }
            return false;
        }

        public dynamic GetBlackListModelList(string zoneId, string authEmail, string authKey, int limit, int offset, string ip, DateTime start, DateTime end, string notes)
        {
            string key = $"GetBlackListModelList:{zoneId}-{authEmail}-{authKey}";
            var query = Utils.GetMemoryCache<List<BlackListModel>>(key, () =>
            {
                var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, EnumMode.challenge);
                return list.Select(a => new BlackListModel
                {
                    IP = a.configurationValue,
                    CreateTime = a.createTime.ToString("MM/dd/yyyy HH:mm:ss"),
                    Notes = a.notes,
                }).ToList();
            }, 5).AsQueryable();

            //var list = cloundFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, EnumMode.challenge);
            //var query = list.Select(a => new BlackListModel
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
            //if (Convert.ToDateTime(end.ToString("yyyy-MM-dd HH:mm")) > Convert.ToDateTime(start.ToString("yyyy-MM-dd HH:mm")))
            //{
                if (start != DateTime.MinValue)
                {
                    query = query.Where(a => Convert.ToDateTime( a.CreateTime)>= start);
                }
                if (end != DateTime.MinValue)
                {
                    query = query.Where(a => Convert.ToDateTime(a.CreateTime) <= end);
                }
            //}

            int total = query.Count();
            var rows = query.Skip(offset).Take(limit).ToList();
            return new {total, rows = rows };
        }
    }
}
