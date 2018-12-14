using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface IActiveReportService
    {
        void GeneratedActiveReport();
    }
    public class ActiveReportService : IActiveReportService
    {
        private readonly string nothing = "Not Applicable";
        private readonly ILogService logService = new LogService();
        private bool isDebug = true;
        private int oneDayCount = 2;

        private static object obj_Sync = new object();
        private static IActiveReportService activeReportService;
        private bool isProcessing = false;
        private List<string> _suffixList;

        private ActiveReportService()
        {
            var filterSuffixList = ConfigurationManager.AppSettings["FilterSuffixList"];
            if (!string.IsNullOrEmpty(filterSuffixList))
            {
                _suffixList = filterSuffixList.Split(',').ToList();
            }

#if DEBUG
#else
            isDebug = false;
            oneDayCount = 24;
#endif
        }
        public static IActiveReportService GetInstance()
        {
            if (activeReportService == null)
            {
                lock (obj_Sync)
                {
                    activeReportService = new ActiveReportService();
                }
            }

            return activeReportService;
        }

        public void GeneratedActiveReport()
        {
            try
            {
                if (!isProcessing)
                {
                    isProcessing = true;
                    if (isDebug)
                    {
                        TestingEnvironment();
                    }
                    else
                    {
                        ProductionEnvironment();
                    }
                    isProcessing = false;
                }
            }
            catch (Exception e)
            {
                logService.Error(e);
                isProcessing = false;
            }
            finally
            {

            }

        }


        private void TestingEnvironment()
        {
            List<SmtpQueue> smtpQueues = SmtpQueueBusiness.GetList();
            SmtpQueue lastReport = smtpQueues.OrderBy(a => a.Id).LastOrDefault();
            DateTime date = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy 00:00:00"));
            string title = date.ToString("MM/dd/yyyy 23");
            if (lastReport != null && lastReport.Title == title)
            {
                //已经生成了报表
            }
            else
            {
                string lastTitle = lastReport?.Title;
                if (string.IsNullOrEmpty(lastTitle))
                {
                    title = date.ToString("MM/dd/yyyy HH");
                }
                else
                {
                    DateTime lastDate = DateTime.Parse(lastTitle + ":00:00");
                    date = lastDate.AddHours(1);
                    title = date.ToString("MM/dd/yyyy HH");
                }

                //// 每天9开始统计前一天的数据
                //if (DateTime.Now > Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 09:00:00")))
                //{
                List<ZoneEntity> zoneEntities = ZoneBusiness.GetZoneList().Where(a => a.IfEnable).ToList();
                if (zoneEntities != null && zoneEntities.Count > 0)
                {
                    foreach (ZoneEntity zone in zoneEntities)
                    {
                        CreateActiveReportZoneTesting(zone, date);
                    }
                    //插入邮件发送队列
                    SmtpQueueBusiness.Add(new SmtpQueue
                    {
                        Title = title,
                        Status = 0,
                        CreatedTime = DateTime.Now,
                        SendedTime = DateTime.Now,
                        Remark = "",
                    });
                }
            }
            //}

        }
        private void ProductionEnvironment()
        {
            List<SmtpQueue> smtpQueues = SmtpQueueBusiness.GetList();
            SmtpQueue lastReport = smtpQueues.OrderBy(a => a.Id).LastOrDefault();
            DateTime date = DateTime.Now.AddDays(-1);
            string title = date.ToString("MM/dd/yyyy");
            if (lastReport != null && lastReport.Title == title)
            {
                //已经生成了报表
            }
            else
            {
                //// 每天9开始统计前一天的数据
                //if (DateTime.Now > Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 09:00:00")))
                //{
                List<ZoneEntity> zoneEntities = ZoneBusiness.GetZoneList().Where(a => a.IfEnable).ToList();
                if (zoneEntities != null && zoneEntities.Count > 0)
                {
                    foreach (ZoneEntity zone in zoneEntities)
                    {
                        CreateActiveReportZoneProduction(zone, date);
                    }
                    //插入邮件发送队列
                    SmtpQueueBusiness.Add(new SmtpQueue
                    {
                        Title = title,
                        Status = 0,
                        CreatedTime = DateTime.Now,
                        SendedTime = DateTime.Now,
                        Remark = "",
                    });
                }
            }
            //}

        }

        private void CreateActiveReportZoneTesting(ZoneEntity zone, DateTime date)
        {
            string title = date.ToString("MM/dd/yyyy HH");
            string zoneId = zone.ZoneId;
            //24个小时，取第一分钟的数据
            List<List<CloudflareLog>> cloudflareLogs = new List<List<CloudflareLog>>();

            for (int i = 0; i < oneDayCount; i++)
            {
                double sample = 1;
                DateTime startTime = Convert.ToDateTime(date.ToString(string.Format("yyyy-MM-dd HH:{0}:00", i.ToString("00"))));
                DateTime endTime = Convert.ToDateTime(date.ToString(string.Format("yyyy-MM-dd HH:{0}:00", (i+1).ToString("00"))));
                string authEmail = zone.AuthEmail;
                string authKey = zone.AuthKey;
                string key = string.Format("{0}-{1}-{2}-{3}", startTime.ToString("yyyyMMddHHmmss"), endTime.ToString("yyyyMMddHHmmss"), sample, zoneId);

                List<CloudflareLog> logs = new List<CloudflareLog>();
                //if (isDebug)
                //{
                //    //logs.Add(new CloudflareLog {
                //    //    ClientIP="xxxxx",
                //    //    ClientRequestHost="aaaa",
                //    //});
                //    //logService.Debug(JsonConvert.SerializeObject(logs));
                //    //string xx = Utils.GetFileContext("Logs_20181207.txt");
                //    //string jf = xx.Substring(xx.IndexOf("[{"));
                //    string log = Utils.GetFileContext("logs-debug.txt");
                //    logs = JsonConvert.DeserializeObject<List<CloudflareLog>>(log.Substring(log.IndexOf("[{")));
                //}
                //else
                //{
                    ICloudflareLogHandleSercie cloudflareLogHandleSercie = new CloudflareLogHandleSercie(zoneId, authEmail, authKey, sample, startTime, endTime);
                    cloudflareLogHandleSercie.TaskStart();
                    logs = cloudflareLogHandleSercie.GetCloudflareLogs(key)
                        .Where(a => !IfInSuffixList(a.ClientRequestURI)).ToList();
                //}

                cloudflareLogs.Add(logs);
                logService.Debug(key);
            }

            GeneratedActiveReport(title, zone, cloudflareLogs);
            GeneratedWhiteListReport(title, zone, cloudflareLogs);
        }
        private void CreateActiveReportZoneProduction(ZoneEntity zone, DateTime date)
        {
            string title = date.ToString("MM/dd/yyyy");
            string zoneId = zone.ZoneId;

            var list = ActionReportBusiness.GetListByTitle(title);
            if (list != null && list.Count(a => a.ZoneId == zoneId) > 0)
            {
                //如果本时段的数据已经存在则不必重复生成
                return;
            }

            //24个小时，取第一分钟的数据
            List<List<CloudflareLog>> cloudflareLogs = new List<List<CloudflareLog>>();

            for (int i = 0; i < oneDayCount; i++)
            {
                double sample = 1;
                DateTime startTime = Convert.ToDateTime(date.ToString(string.Format("yyyy-MM-dd {0}:00:00", i.ToString("00"))));
                DateTime endTime = Convert.ToDateTime(date.ToString(string.Format("yyyy-MM-dd {0}:01:00", i.ToString("00"))));
                string authEmail = zone.AuthEmail;
                string authKey = zone.AuthKey;
                string key = string.Format("{0}-{1}-{2}-{3}", startTime.ToString("yyyyMMddHHmmss"), endTime.ToString("yyyyMMddHHmmss"), sample, zoneId);

                List<CloudflareLog> logs = new List<CloudflareLog>();
                if (isDebug)
                {
                    //logs.Add(new CloudflareLog {
                    //    ClientIP="xxxxx",
                    //    ClientRequestHost="aaaa",
                    //});
                    //logService.Debug(JsonConvert.SerializeObject(logs));
                    //string xx = Utils.GetFileContext("Logs_20181207.txt");
                    //string jf = xx.Substring(xx.IndexOf("[{"));
                    string log = Utils.GetFileContext("logs-debug.txt");
                    logs = JsonConvert.DeserializeObject<List<CloudflareLog>>(log.Substring(log.IndexOf("[{")));
                }
                else
                {
                    ICloudflareLogHandleSercie cloudflareLogHandleSercie = new CloudflareLogHandleSercie(zoneId, authEmail, authKey, sample, startTime, endTime);
                    cloudflareLogHandleSercie.TaskStart();
                    logs = cloudflareLogHandleSercie.GetCloudflareLogs(key)
                        .Where(a => !IfInSuffixList(a.ClientRequestURI)).ToList();
                }            
                
                cloudflareLogs.Add(logs);
                logService.Debug(key);
            }

            //为了防止异常导致之前已经存储进去的数据重复，先删除对应的数据
            ActionReportBusiness.Delete(zoneId, title);

            GeneratedActiveReport(title, zone, cloudflareLogs);
            GeneratedWhiteListReport(title, zone, cloudflareLogs);
        }

        private int GetMax(List<List<CloudflareLog>> cloudflareLogs, string ip, string hostName)
        {
            int max = 0;
            foreach(List<CloudflareLog> logs in cloudflareLogs)
            {
                var subCloudflareLogs = logs.Where(a => a.ClientIP == ip && a.ClientRequestHost == hostName);
                int count = subCloudflareLogs.Count();
                if (count > max)
                {
                    max = count;
                }
            }
            return max;
        }
        private int GetMin(List<List<CloudflareLog>> cloudflareLogs, string ip, string hostName)
        {
            int min = int.MaxValue;
            foreach (List<CloudflareLog> logs in cloudflareLogs)
            {
                var subCloudflareLogs = logs.Where(a => a.ClientIP == ip && a.ClientRequestHost == hostName);
                int count = subCloudflareLogs.Count();
                if (count < min)
                {
                    min = count;
                }
            }
            return min;
        }
        private int GetAvg(List<List<CloudflareLog>> cloudflareLogs, string ip, string hostName)
        {
            int avg = 0;
            int sum = 0;
            foreach (List<CloudflareLog> logs in cloudflareLogs)
            {
                var subCloudflareLogs = logs.Where(a => a.ClientIP == ip && a.ClientRequestHost == hostName);
                int count = subCloudflareLogs.Count();
                sum += count;
            }
            avg = sum / oneDayCount;
            return avg;
        }
        private List<string> GetTop5Urls(List<List<CloudflareLog>> cloudflareLogs, string ip, string hostName)
        {
            List<string> top5List = new List<string>();
            var totalList = cloudflareLogs.SelectMany(a => a)
                .Where(a => a.ClientIP == ip && a.ClientRequestHost == hostName)
                .GroupBy(a => a.ClientRequestURI).Select(g => new
                {
                    Count = g.Count(),
                    FullUrl = string.Format("{0}{1}", hostName, g.Key)
                })
                .OrderByDescending(a => a.Count)
                .Take(5);
            foreach(var item in totalList)
            {
                top5List.Add(string.Format("{0}(Avg:{1})", item.FullUrl, Math.Ceiling(item.Count / (float)oneDayCount)));
            }
            return top5List;
        }
        private void GeneratedActiveReport(string title, ZoneEntity zone, List<List<CloudflareLog>> cloudflareLogs)
        {
            var cloundFlareApiService = new CloundFlareApiService();
            var whiteList = cloundFlareApiService.GetAccessRuleList(zone.ZoneId, zone.AuthEmail, zone.AuthKey, EnumMode.whitelist);
            var whiteListIps = whiteList.Select(a => a.configurationValue);

            var totalList = cloudflareLogs.SelectMany(a => a)
                                          .GroupBy(a => new { a.ClientIP, a.ClientRequestHost })
                                          .Select(
                                                    g => new
                                                    {
                                                        IP = g.Key.ClientIP,
                                                        HostName = g.Key.ClientRequestHost,
                                                        Count = g.Count(),
                                                    })
                                           .Where(a=>!whiteListIps.Contains(a.IP))
                                           .OrderByDescending(a => a.Count);

            List<string> ipList = new List<string>();

            foreach (var item in totalList)
            {
                //取不同的20个IP
                if (!ipList.Contains(item.IP))
                {
                    ipList.Add(item.IP);
                }

                if (ipList.Count > 20)
                {
                    break;
                }

                int max = GetMax(cloudflareLogs, item.IP, item.HostName);
                int min = GetMin(cloudflareLogs, item.IP, item.HostName);
                int avg = GetAvg(cloudflareLogs, item.IP, item.HostName);
                List<string> urls = GetTop5Urls(cloudflareLogs, item.IP, item.HostName);
                string urlsJson = JsonConvert.SerializeObject(urls);

                int? maxHistory = ActionReportBusiness.GetMaxForAction(zone.ZoneId, item.IP, item.HostName);
                int? minHistory = ActionReportBusiness.GetMinForAction(zone.ZoneId, item.IP, item.HostName);
                int? avgHistory = ActionReportBusiness.GetAvgForAction(zone.ZoneId, item.IP, item.HostName);

                string maxDisplay = string.Format("{0}({1})", max, maxHistory.HasValue ? maxHistory.Value.ToString() : nothing);
                string minDisplay = string.Format("{0}({1})", min, minHistory.HasValue ? minHistory.Value.ToString() : nothing);
                string avgDisplay = string.Format("{0}({1})", avg, avgHistory.HasValue ? avgHistory.Value.ToString() : nothing);

                ActionReport report = new ActionReport
                {
                    IP = item.IP,
                    HostName = item.HostName,
                    Max = max,
                    Min = min,
                    Avg = avg,
                    FullUrl = urlsJson,
                    Title = title,
                    ZoneId = zone.ZoneId,
                    Count = item.Count,
                    Mode = "Action",
                    CreatedTime = DateTime.UtcNow,
                    MaxDisplay = maxDisplay,
                    MinDisplay = minDisplay,
                    AvgDisplay = avgDisplay,
                    Remark = "",
                };
                ActionReportBusiness.Add(report);
            }
        }
        private void GeneratedWhiteListReport(string title, ZoneEntity zone, List<List<CloudflareLog>> cloudflareLogs)
        {
            var cloundFlareApiService = new CloundFlareApiService();
            var whiteList = cloundFlareApiService.GetAccessRuleList(zone.ZoneId, zone.AuthEmail, zone.AuthKey, EnumMode.whitelist);
            var subWhiteList = whiteList.Where(a => a.notes.Contains("WHITELIST CLEINT'S IP ADDRESS SITEID"))
                                        .Select(a => new WhiteListModel
                                        {
                                            IP = a.configurationValue
                                        }).ToList();

            //var subWhiteList = new List< WhiteListModel>(){
            //    new WhiteListModel {
            //        IP= "131.242.135.253",
            //    },
            //    new WhiteListModel {
            //        IP= "131.242.135.252",
            //    }
            //}; 

            var totalList = cloudflareLogs.SelectMany(a => a)
                .Join(subWhiteList,
                left => left.ClientIP,
                right => right.IP,
                (left, right) => new
                {
                    left.ClientIP,
                    left.ClientRequestHost
                })
                .GroupBy(a => new { a.ClientIP, a.ClientRequestHost })
                .Select(
                        g => new
                        {
                            IP = g.Key.ClientIP,
                            HostName = g.Key.ClientRequestHost,
                            Count = g.Count(),
                        })
               .OrderByDescending(a => a.Count);

            List<string> ipList = new List<string>();

            foreach (var item in totalList)
            {
                //取不同的20个IP
                if (!ipList.Contains(item.IP))
                {
                    ipList.Add(item.IP);
                }

                if (ipList.Count > 20)
                {
                    break;
                }

                int max = GetMax(cloudflareLogs, item.IP, item.HostName);
                int min = GetMin(cloudflareLogs, item.IP, item.HostName);
                int avg = GetAvg(cloudflareLogs, item.IP, item.HostName);
                List<string> urls = GetTop5Urls(cloudflareLogs, item.IP, item.HostName);
                string urlsJson = JsonConvert.SerializeObject(urls);

                int? maxHistory = ActionReportBusiness.GetMaxForWhiteList(zone.ZoneId, item.IP, item.HostName);
                int? minHistory = ActionReportBusiness.GetMinForWhiteList(zone.ZoneId, item.IP, item.HostName);
                int? avgHistory = ActionReportBusiness.GetAvgForWhiteList(zone.ZoneId, item.IP, item.HostName);

                string maxDisplay = string.Format("{0}({1})", max, maxHistory.HasValue ? maxHistory.Value.ToString() : nothing);
                string minDisplay = string.Format("{0}({1})", min, minHistory.HasValue ? minHistory.Value.ToString() : nothing);
                string avgDisplay = string.Format("{0}({1})", avg, avgHistory.HasValue ? avgHistory.Value.ToString() : nothing);

                ActionReport report = new ActionReport
                {
                    IP = item.IP,
                    HostName = item.HostName,
                    Max = max,
                    Min = min,
                    Avg = avg,
                    FullUrl = urlsJson,
                    Title = title,
                    ZoneId = zone.ZoneId,
                    Count = item.Count,
                    Mode = "WhiteList",
                    CreatedTime = DateTime.UtcNow,
                    MaxDisplay = maxDisplay,
                    MinDisplay = minDisplay,
                    AvgDisplay = avgDisplay,
                    Remark = "",
                };
                ActionReportBusiness.Add(report);
            }
        }

        private bool IfInSuffixList(string requestUrl)
        {
            foreach (var suffix in _suffixList)
            {
                if (requestUrl.ToLower().EndsWith($".{suffix}"))
                    return true;
            }

            return false;
        }
    }
    
}
