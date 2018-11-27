using AttackPrevent.Business;
using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService.Job
{
    public class LogAnalyzeJob : IJob
    {
        private double sample = 1;
        private int globalThreshold = 200;
        private int globalPeriod = 60;
        private int timeSpan = 60;
        private List<HostConfiguration> hostConfigList = null;

        private List<RateLimitEntity> GetRateLimitEntities()
        {
            List<RateLimitEntity> list = new List<RateLimitEntity>();
            list.Add(new RateLimitEntity() {
                ID =1,
                Period = 60,
                Threshold =5,
                Url= "chatserver.comm100.com/prechat.aspx",
                OrderNo = 1,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 2,
                Period = 60,
                Threshold = 5,
                Url = "chatserver.comm100.com/offlinemessage.aspx",
                OrderNo = 2,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 3,
                Period = 60,
                Threshold = 20,
                Url = "chatserver.comm100.com/campaign.ashx",
                OrderNo = 3,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 4,
                Period = 60,
                Threshold = 20,
                Url = "chatserver.comm100.com/livechat.ashx",
                OrderNo = 4,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 5,
                Period = 60,
                Threshold = 20,
                Url = "chatserver.comm100.com/livechatjs.ashx",
                OrderNo = 5,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 6,
                Period = 60,
                Threshold = 20,
                Url = "chatserver.comm100.com/chatbutton.aspx",
                OrderNo = 6,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 7,
                Period = 60,
                Threshold = 20,
                Url = "chatserver.comm100.com/bbs.aspx",
                OrderNo = 7,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 8,
                Period = 60,
                Threshold = 2,
                Url = "chatserver.comm100.com/chatwindowmobile.aspx",
                OrderNo = 8,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 9,
                Period = 60,
                Threshold = 2,
                Url = "chatserver.comm100.com/chatwindowembedded.aspx",
                OrderNo = 9,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 10,
                Period = 60,
                Threshold = 10,
                Url = "chatserver.comm100.com/chatwindow.aspx",
                OrderNo = 10,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 11,
                Period = 600,
                Threshold = 2,
                Url = "www.comm100.com/secure/register.ashx",
                OrderNo = 11,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });

            list.Add(new RateLimitEntity()
            {
                ID = 12,
                Period = 60,
                Threshold = 5,
                Url = "chatserver.comm100.com/chatwindow.aspx",
                OrderNo = 12,
                RateLimitTriggerIpCount = 5,
                EnlargementFactor = 3
            });
            return list;
        }
    
        public Task Execute(IJobExecutionContext context)
        {
            #region 设置全局参数
            var globalConfigurations = GlobalConfigurationBusiness.GetConfigurationList();
            if (null != globalConfigurations && globalConfigurations.Count > 0)
            {
                globalThreshold = globalConfigurations[0].GlobalThreshold;
                globalPeriod = globalConfigurations[0].GlobalPeriod;
                sample = globalConfigurations[0].GlobalSample;
                timeSpan = globalConfigurations[0].GlobalTimeSpan;
            };
            #endregion

            hostConfigList = HostConfigurationBusiness.GetList();

            var zoneList = ZoneBusiness.GetAllList();
            var zoneEntity = null != zoneList && zoneList.Count > 0 ? zoneList[0] : new ZoneEntity()
            {
                ZoneName = "comm100.com",
                ZoneId = "2068c8964a4dcef78ee5103471a8db03",
                AuthEmail = "elei.xu@comm100.com",
                AuthKey = "1e26ac28b9837821af730e70163f0604b4c35",
                IfTestStage = true
            };

            #region Start Analyze Log
            StartAnalyze(zoneEntity);
            #endregion

            return Task.FromResult(0);
        }

        private void StartAnalyze(ZoneEntity zoneEntity)
        {
            var dtNow = DateTime.Now;
            var dtStart = dtNow.AddMinutes(-7).AddSeconds(0-dtNow.Second);
            var dtEnd = dtStart.AddMinutes(2);

            var timeStageList = new List<KeyValuePair<DateTime, DateTime>>();
            while (true)
            {
                timeStageList.Add(new KeyValuePair<DateTime, DateTime>(dtStart, dtStart.AddSeconds(timeSpan)));

                dtStart = dtStart.AddSeconds(timeSpan);

                if (dtStart >= dtEnd)
                {
                    break;
                }
            }
            var cloudflare = new CloudflareBusiness(zoneEntity.ZoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);
            var ipWhiteList = cloudflare.GetIpWhitelist();
            var cloudflareLogs = new List<CloudflareLog>();
            var rateLimits = GetRateLimitEntities();

            foreach (var keyValuePair in timeStageList)
            {
                var retryCount = 0;
                dtStart = keyValuePair.Key;
                dtEnd = keyValuePair.Value;

                var timeStage = string.Format("{0}]-[{1}", dtStart.ToString("MM/dd/yyyy HH:mm:ss"), dtEnd.ToString("MM/dd/yyyy HH:mm:ss"));
                AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("Start to get logs, time range is [{0}].", timeStage)));
                
                cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var retry);

                while (retry && retryCount < 10)
                {
                    retryCount++;
                    cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
                }

                AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("Finished to get total [{1}] records, time range is [{0}].", timeStage, cloudflareLogs.Count)));

                if (cloudflareLogs.Count > 0)
                {
                    #region 分析日志
                    var requestDetailList = cloudflareLogs.Where(x => !ipWhiteList.Contains(x.ClientIP)).Select(x => new LogAnalyzeModel()
                    {
                        IP = x.ClientIP,
                        RequestHost = x.ClientRequestHost,
                        RequestFullUrl = string.Format("{0}{1}", x.ClientRequestHost, x.ClientRequestURI),
                        RequestUrl = string.Format("{0}{1}", x.ClientRequestHost, x.ClientRequestURI.IndexOf('?') > 0 ? x.ClientRequestURI.Substring(0, x.ClientRequestURI.IndexOf('?')) : x.ClientRequestURI)
                    }).ToList();

                    //AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("除去白名单后日志总计 [{0}] 条", requestDetailList.Count)));

                    if (requestDetailList.Count > 0)
                    {
                        AnalyzeLog(requestDetailList, zoneEntity, rateLimits, timeStage);
                    }
                    
                    #endregion
                }
            }
        }

        private void AnalyzeLog(List<LogAnalyzeModel> logsAll, ZoneEntity zoneEntity, List<RateLimitEntity> rateLimits, string timeStage)
        {
            var systemLogList = new List<AuditLogEntity>();
            var zoneId = zoneEntity.ZoneId;
            var ifTestStage = zoneEntity.IfTestStage;
            var ifAttacking = false;
            try
            {
                CloudflareAccessRuleResponse cloudflareAccessRuleResponse = null;
                var cloudflare = new CloudflareBusiness(zoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("Start analyzing logs, time range is [{0}].", timeStage)));
                var dtNow = DateTime.Now;

                #region Analyze log by host access exceed
                var logsIpAll = logsAll.GroupBy(x => new { x.IP, x.RequestHost }).Select(x => new LogAnalyzeModel()
                {
                    IP = x.Key.IP,
                    RequestHost = x.Key.RequestHost,
                    RequestCount = x.Count()
                }).Where(x => IfOverHostRequestLimit(x.RequestHost, x.RequestCount)
                ).ToList().OrderByDescending(x => x.RequestCount).ThenBy(x => x.RequestHost);

                if (logsIpAll.Count() > 0)
                {
                    ifAttacking = true;
                    ZoneBusiness.UpdateAttackFlag(true, zoneId);
                    //systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("Suspected of an attack, modified the attack token and trigger an alert.", zoneEntity.ZoneName)));

                    var sbDetail = new StringBuilder();
                    sbDetail.AppendFormat("[{1}] IPs exceeded the host access threshold, time range is [{0}].<br />", timeStage, logsIpAll.Count());
                    foreach (var rule in logsIpAll)
                    {
                        sbDetail.AppendFormat("IP [{0}] visited [{1}] total ({2} times)]; <br />", rule.IP, rule.RequestHost, rule.RequestCount);
                    }

                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbDetail.ToString()));


                    var currentHostConfigList = new List<HostConfiguration>();
                    foreach (var rule in logsIpAll)
                    {
                        sbDetail = new StringBuilder();
                        currentHostConfigList = hostConfigList.Where(x => x.Host.Equals(rule.RequestHost)).ToList();
                        sbDetail.AppendFormat("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded host access threshold(Period=[{4}],Threshold=[{5}])，details(only list the top 10 records)：<br />", 
                            rule.IP, timeStage, rule.RequestHost, rule.RequestCount, 
                            currentHostConfigList.Count > 0 ? currentHostConfigList[0].Period : globalPeriod, currentHostConfigList.Count > 0 ? currentHostConfigList[0].Threshold : globalThreshold);

                        var ipRequestList = logsAll.Where(x => x.IP.Equals(rule.IP)).GroupBy(x => new { x.RequestFullUrl }).Select(x => new LogAnalyzeModel()
                        {
                            RequestFullUrl = x.Key.RequestFullUrl,
                            RequestCount = x.Count()
                        }).ToList().OrderByDescending(x => x.RequestCount).ToList();

                        for (var index = 0; index < Math.Min(ipRequestList.Count(), 10); index++)
                        {
                            sbDetail.AppendFormat("[{0}] {1} times.<br />", ipRequestList[index].RequestFullUrl, ipRequestList[index].RequestCount);
                        }

                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbDetail.ToString()));

                        if (ifTestStage)
                        {
                            sbDetail.AppendFormat("Ban IP [{0}] successfully.<br />", rule.IP);
                        }
                        else
                        {
                            cloudflareAccessRuleResponse = cloudflare.BanIp(rule.IP, "Ban Ip By Attack Prevent Windows service!");
                            if (cloudflareAccessRuleResponse.Success)
                            {
                                sbDetail.AppendFormat("Ban IP [{0}] successfully.<br />", rule.IP);
                            }
                            else
                            {
                                sbDetail.AppendFormat("Ban IP [{0}] failure, the reason is：[{1}].<br />", rule.IP, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "Cloudflare does not return any error message");
                            }
                        }

                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, sbDetail.ToString()));
                    }
                }

                //抽取出所有ratelimit规则中的请求列表
                var logs = logsAll.Select(x => new LogAnalyzeModel()
                {
                    IP = x.IP,
                    RequestHost = x.RequestHost,
                    RequestFullUrl = string.Format("{0}{1}", x.RequestHost, x.RequestUrl),
                    RequestUrl = string.Format("{0}{1}", x.RequestHost, x.RequestUrl.IndexOf('?') > 0 ? x.RequestUrl.Substring(0, x.RequestUrl.IndexOf('?')) : x.RequestUrl)
                }).Where(x => IfInRateLimitRule(x.RequestUrl, rateLimits)).ToList();
                #endregion

                #region Analyze log by rate limit rules
                //foreach (var rateLimit in rateLimits)
                //{
                //    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("Start analyzing rule [ID=[{0}],Url=[{1}],Period=[{2}],Threshold=[{3}]]", rateLimit.ID, rateLimit.Url, rateLimit.Period, rateLimit.Threshold)));
                //    //抽取出所有ratelimit规则中的请求列表
                //    var logAnalyzeDetailList = logs.Where(x => x.RequestUrl.Equals(rateLimit.Url)).ToList();

                //    //对IP的请求地址(不包含querystring)进行分组
                //    var ipRequestListIncludingQueryString = logAnalyzeDetailList.GroupBy(x => new { x.IP, x.RequestUrl, x.RequestFullUrl }).Select(x => new LogAnalyzeModel()
                //    {
                //        IP = x.Key.IP,
                //        RequestUrl = x.Key.RequestUrl,
                //        RequestFullUrl = x.Key.RequestFullUrl,
                //        RequestCount = x.Count()
                //    }).ToList();

                //    //对IP的请求地址(不包含querystring)进行分组
                //    var ipRequestList = logAnalyzeDetailList.GroupBy(x => new { x.IP, x.RequestUrl }).Select(x => new LogAnalyzeModel()
                //    {
                //        IP = x.Key.IP,
                //        RequestUrl = x.Key.RequestUrl,
                //        RequestCount = x.Count()
                //    }).ToList();

                //    //抽取出所有违反规则的IP请求列表
                //    var brokenRuleIpList = (from item in ipRequestList
                //                            where item.RequestUrl.ToLower().Equals(rateLimit.Url.ToLower())
                //                                  && item.RequestCount / (float)(timeSpan * sample) >= (rateLimit.Threshold * rateLimit.EnlargementFactor / (float)rateLimit.Period)
                //                            select new LogAnalyzeModel()
                //                            {
                //                                IP = item.IP,
                //                                RequestUrl = item.RequestUrl,
                //                                RequestCount = item.RequestCount,
                //                                RateLimitId = rateLimit.ID,
                //                                RateLimitTriggerIpCount = rateLimit.RateLimitTriggerIpCount
                //                            }).ToList();


                //    var brokenIpCountList = brokenRuleIpList.GroupBy(x => new { x.RateLimitId, x.RequestUrl, x.RateLimitTriggerIpCount }).Select(x => new LogAnalyzeModel()
                //    {
                //        RateLimitTriggerIpCount = x.Key.RateLimitTriggerIpCount,
                //        RateLimitId = x.Key.RateLimitId,
                //        RequestUrl = x.Key.RequestUrl,
                //        RequestCount = x.Count()
                //    }).ToList();

                //    //抽取出超过IP数量的规则列表，需要新增或OPEN Cloudflare的Rate Limiting Rule
                //    var brokenRuleList = brokenIpCountList.Where(x => x.RateLimitTriggerIpCount <= x.RequestCount).ToList();


                //    if (brokenRuleList.Count > 0)
                //    {
                //        ifAttacking = true;
                //        ZoneBusiness.UpdateAttackFlag(true, zoneId);
                //        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("Suspected of an attack in ZoneName [{0}], modified the attack token and trigger an alert.", zoneEntity.ZoneName)));

                //        var sbDetail = new StringBuilder(string.Format("Exceeded rate limiting threshold(Threshold=[{0}],Period=[{1}]), details：<br />", rateLimit.Threshold, rateLimit.Period));
                //        foreach (var rule in logsIpAll)
                //        {
                //            sbDetail.AppendFormat("[{0}] {1} times.<br /> ", rule.IP, rule.RequestCount);
                //        }
                //        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbDetail.ToString()));

                //        foreach (var rule in brokenRuleIpList)
                //        {
                //            sbDetail = new StringBuilder();
                //            var ipRequestUrlList = ipRequestListIncludingQueryString.Where(x => x.IP.Equals(rule.IP)).ToList().OrderByDescending(x => x.RequestCount);
                //            foreach (var ipRequestUrl in ipRequestUrlList)
                //            {
                //                sbDetail.AppendFormat("IP [{0}] visited [{2}] [{3}] times, time range is [{1}].<br />", rule.IP, timeStage, ipRequestUrl.RequestFullUrl, ipRequestUrl.RequestCount);
                //            }
                //        }
                //        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbDetail.ToString()));

                //        sbDetail.AppendFormat("Start to open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]].<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                //        if (ifTestStage)
                //        {
                //            sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                //        }
                //        else
                //        {

                //            if (cloudflare.OpenRateLimit(rateLimit.Url, rateLimit.Threshold, rateLimit.Period, out var openRateLimitLogs))
                //            {
                //                sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                //            }
                //            else
                //            {
                //                foreach (var errorLog in openRateLimitLogs)
                //                {
                //                    sbDetail.AppendFormat(errorLog.Detail).Append("<br />");
                //                }
                //            }
                //        }

                //        foreach (var rule in brokenRuleIpList)
                //        {
                //            if (ifTestStage)
                //            {
                //                sbDetail.AppendFormat("Ban IP [{0}] successfully.<br />", rule.IP);
                //            }
                //            else
                //            {
                //                cloudflareAccessRuleResponse = cloudflare.BanIp(rule.IP, "Ban Ip By Attack Prevent Windows service!");
                //                if (cloudflareAccessRuleResponse.Success)
                //                {
                //                    sbDetail.AppendFormat("Ban IP [{0}] successfully.<br />", rule.IP);
                //                }
                //                else
                //                {
                //                    sbDetail.AppendFormat("Ban IP [{0}] failure, the reason is：[{1}].<br />", rule.IP, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "Cloudflare没有返回错误信息");
                //                }
                //            }
                //        }
                //        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, sbDetail.ToString()));
                //    }

                //    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("Finished to analyze rule [ID={0},Url={1}].", rateLimit.ID, rateLimit.Url)));
                //}
                #endregion

                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("Finished to analyze logs, time range is [{0}].", timeStage)));

            }
            catch (Exception ex)
            {
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Error, string.Format("Error in analyzing logs, time range is [{1}], the reason is:[{0}]", ex.Message, timeStage)));
            }
            finally
            {
                AuditLogBusiness.AddList(systemLogList);
                if (!ifAttacking)
                {
                    ZoneBusiness.UpdateAttackFlag(false,zoneId);

                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("There's no attack ,cancle the alert call in ZoneName [{0}].", zoneEntity.ZoneName)));
                }
            }
        }

        private bool IfInRateLimitRule(string requestUrl, List<RateLimitEntity> rateLimits)
        {
            if (null != rateLimits)
            {
                var result = rateLimits.Where(x => requestUrl.Equals(x.Url));
                if (result != null && result.Count() > 0) return true;

            }
            return false;
        }

        private bool IfOverHostRequestLimit(string hostStr, int requestCount)
        {
            foreach (var hostConfig in hostConfigList)
            {
                if (hostConfig.Host.Equals(hostStr))
                {
                    return requestCount / (float)(timeSpan * sample) >= ((float)hostConfig.Threshold / hostConfig.Period);
                }
            }
            return requestCount / (float)(timeSpan * sample) >= ((float)globalThreshold / globalPeriod);
        }
    }
}
