using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface IActiveReportMailService
    {
        string GeneratedMail(string title);
        void GeneratedActiveReport();
    }
    public class ActiveReportMailService: IActiveReportMailService
    {
        private readonly string nothing = "Not Applicable";

        public void GeneratedActiveReport()
        {
            List<SmtpQueue> smtpQueues = SmtpQueueBusiness.GetList();
            SmtpQueue lastReport = smtpQueues.OrderBy(a => a.Id).LastOrDefault();
            DateTime date = DateTime.Now.AddDays(-1);
            if (lastReport.Title == date.ToString("MM/dd/yyyy"))
            {
                //已经生成了报表
            }
            else
            {
                // 每天9开始统计前一天的数据
                if(DateTime.Now > Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 09:00:00")))
                {
                    List<ZoneEntity> zoneEntities = ZoneBusiness.GetZoneList().Where(a => a.IfEnable && a.IfAttacking && !a.IfTestStage).ToList();
                    foreach (ZoneEntity zone in zoneEntities)
                    {
                        CreateActiveReportZone(zone, date);
                    }
                }
            }
        }

        private void CreateActiveReportZone(ZoneEntity zone, DateTime date)
        {
            string title = date.ToString("MM/dd/yyyy");
            string zoneId = zone.ZoneId;
            //24个小时，取第一分钟的数据
            List<List<CloudflareLog>> cloudflareLogs = new List<List<CloudflareLog>>();
            for (int i = 0; i < 24; i++)
            {
                double sample = 1;
                DateTime startTime = Convert.ToDateTime(date.ToString(string.Format("yyyy-MM-dd {0}:00:00", i.ToString("00"))));
                DateTime endTime = Convert.ToDateTime(date.ToString(string.Format("yyyy-MM-dd {0}:01:00", i.ToString("00"))));
                string authEmail = zone.AuthEmail;
                string authKey = zone.AuthKey;
                string key = string.Format("{0}-{1}-{2}-{3}", startTime.ToString("yyyyMMddHHmmss"), endTime.ToString("yyyyMMddHHmmss"), sample, zoneId);

                ICloudflareLogHandleSercie cloudflareLogHandleSercie = new CloudflareLogHandleSercie(zoneId, authEmail, authKey, sample, startTime, endTime);
                cloudflareLogHandleSercie.TaskStart();
                List<CloudflareLog> logs = cloudflareLogHandleSercie.GetCloudflareLogs(key);
                cloudflareLogs.Add(logs);
            }
            
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
            avg = sum / 24;
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
                top5List.Add(string.Format("{0}(Avg:{1})", item.FullUrl, Math.Ceiling(item.Count / (float)24)));
            }
            return top5List;
        }

        public string GeneratedMail(string title)
        {
            StringBuilder mail = new StringBuilder();
            SmtpQueue smtpQueue = SmtpQueueBusiness.GetByTitle(title);
            if (smtpQueue != null && smtpQueue.Id > 0)
            {
                mail.AppendLine("<div id=\"mail\">");
                List<ActionReport> actionReports = ActionReportBusiness.GetListByTitle(title);

                List<ZoneEntity> zoneEntities = ZoneBusiness.GetZoneList().Where(a => a.IfEnable && a.IfAttacking && !a.IfTestStage).ToList();
                foreach(ZoneEntity zone in zoneEntities)
                {
                    mail.AppendLine("<div>");
                    List<ActionReport> subActionReports = actionReports.Where(a => a.HostName == zone.ZoneName).ToList();
                    mail.AppendLine("<div id=\"mail\">");
                }
                mail.AppendLine("</div>");
            }
            return mail.ToString();
        }

        private string CreateMainZone(string zoneName, List<ActionReport> actionReports)
        {
            StringBuilder mail = new StringBuilder();
            mail.AppendLine("<div>");
            mail.AppendFormat("<p style=\"margin-left:20px; \">{0}</p>",zoneName);
            mail.AppendFormat("<table style=\"border: 1px solid #0094ff; width:95%; min-height: 25px; line-height: 25px; text-align: center; border-collapse: collapse; padding:2px; margin-left:20px;\">");
            mail.AppendFormat("<tr style=\"border: 1px solid #0094ff;\">");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">IP</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:15%;\">Host name</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">Max</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">Min</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">Avg.</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:45%;\">Full URLs</th>");
            mail.AppendLine("</tr>");
            foreach (ActionReport actionReport in actionReports)
            {
                string ip = actionReport.IP;
                string hostName = actionReport.HostName;
                string max = actionReport.MaxDisplay;
                string min = actionReport.MinDisplay;
                string avg = actionReport.AvgDisplay;
                string fullUrls = actionReport.FullUrl;

                List<string> top5UrlList = JsonConvert.DeserializeObject<List<string>>(fullUrls);

                string color = GetBackgroundColor(actionReport);
                mail.AppendFormat("<tr style=\"border: 1px solid #0094ff;{0}\">", color);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>",ip);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", hostName);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", max);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", min);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", avg);
                string urls = "";
                foreach(string url in top5UrlList)
                {
                    urls += string.Format("{0}<br>",url);
                }
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", urls);
                mail.AppendLine("</tr>");
            }
            mail.AppendLine("<div id=\"mail\">");
            return mail.ToString();
        }
        private string GetBackgroundColor(ActionReport actionReport)
        {
            string color = "";
            if(actionReport.MaxDisplay.Contains("Not Applicable") ||
                actionReport.MinDisplay.Contains("Not Applicable") ||
                actionReport.AvgDisplay.Contains("Not Applicable"))
            {
                color = "background-color:red;";
            }
            else
            {
                if (actionReport.MaxDisplay.Contains("("))
                {
                    string[] vls = actionReport.MaxDisplay.Replace(")", "").Split('(');
                    int firNum = Convert.ToInt32(vls[0]);
                    int lstNum = Convert.ToInt32(vls[1]);

                    if(firNum> lstNum)
                    {
                        color = "background-color:red;";
                    }
                }

                if (actionReport.AvgDisplay.Contains("("))
                {
                    string[] vls = actionReport.MaxDisplay.Replace(")", "").Split('(');
                    int firNum = Convert.ToInt32(vls[0]);
                    int lstNum = Convert.ToInt32(vls[1]);

                    if (firNum > lstNum)
                    {
                        color = "background-color:red;";
                    }
                }
            }
            return color;
        }

        private void GeneratedActiveReport(string title, ZoneEntity zone, List<List<CloudflareLog>> cloudflareLogs)
        {
            var totalList = cloudflareLogs.SelectMany(a => a)
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

                int maxHistory = ActionReportBusiness.GetMaxForAction(item.IP, item.HostName);
                int minHistory = ActionReportBusiness.GetMinForAction(item.IP, item.HostName);
                int avgHistory = ActionReportBusiness.GetAvgForAction(item.IP, item.HostName);

                string maxDisplay = string.Format("{0}({1})", max, maxHistory > 0 ? maxHistory.ToString() : nothing);
                string minDisplay = string.Format("{0}({1})", min, minHistory > 0 ? minHistory.ToString() : nothing);
                string avgDisplay = string.Format("{0}({1})", avg, avgHistory > 0 ? avgHistory.ToString() : nothing);

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
                    CreatedTime = DateTime.Now,
                    MaxDisplay = maxDisplay,
                    MinDisplay = minDisplay,
                    AvgDisplay = avgDisplay,
                    Remark = "",
                };
                ActionReportBusiness.Add(report);
            }
        }
        private void GeneratedWhiteListReport(string title , ZoneEntity zone, List<List<CloudflareLog>> cloudflareLogs)
        {
            var cloundFlareApiService = new CloundFlareApiService();
            var whiteList = cloundFlareApiService.GetAccessRuleList(zone.ZoneId, zone.AuthEmail, zone.AuthKey, EnumMode.challenge);
            var subWhiteList = whiteList.Where(a => a.notes.Contains("WHITELIST CLEINT'S IP ADDRESS SITEID"))
                                        .Select(a => new WhiteListModel
                                        {
                                            IP = a.configurationValue
                                        }).ToList();

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

                int maxHistory = ActionReportBusiness.GetMaxForWhiteList(item.IP, item.HostName);
                int minHistory = ActionReportBusiness.GetMinForWhiteList(item.IP, item.HostName);
                int avgHistory = ActionReportBusiness.GetAvgForWhiteList(item.IP, item.HostName);

                string maxDisplay = string.Format("{0}({1})", max, maxHistory > 0 ? maxHistory.ToString() : nothing);
                string minDisplay = string.Format("{0}({1})", min, minHistory > 0 ? minHistory.ToString() : nothing);
                string avgDisplay = string.Format("{0}({1})", avg, avgHistory > 0 ? avgHistory.ToString() : nothing);

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
                    CreatedTime = DateTime.Now,
                    MaxDisplay = maxDisplay,
                    MinDisplay = minDisplay,
                    AvgDisplay = avgDisplay,
                    Remark = "",
                };
                ActionReportBusiness.Add(report);
            }
        }
    }
    
}
