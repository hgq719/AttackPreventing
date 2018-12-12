using AttackPrevent.Business;
using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService.Job
{
    public class LogAnalyzeJob : IJob
    {
        private double _sample = 1;
        private int _timeSpan = 60;
        private int _cancelBanIpTime = 120;
        private List<HostConfigurationEntity> _hostConfigList = null;
        private List<string> _suffixList = new List<string>
        {
            "bmp","ejs","jpeg","pdf","ps","ttf","class","eot","jpg","pict"
            ,"svg","webp","css","eps","js","pls","svgz","woff","csv","gif"
            ,"mid","png","swf","woff2","doc","ico","midi","ppt","tif","xls"
            ,"docx","jar","otf","pptx","tiff","xlsx"
        };
        
        public Task Execute(IJobExecutionContext context)
        {
            #region 设置全局参数
            var globalConfigurations = GlobalConfigurationBusiness.GetConfigurationList();
            if (null != globalConfigurations && globalConfigurations.Count > 0)
            {
                _sample = globalConfigurations[index: 0].GlobalSample;
                _timeSpan = globalConfigurations[index: 0].GlobalTimeSpan;
                _cancelBanIpTime = globalConfigurations[index: 0].CancelBanIPTime;
            };

            var filterSuffixList = ConfigurationManager.AppSettings["FilterSuffixList"];
            if (!string.IsNullOrEmpty(filterSuffixList))
            {
                _suffixList = filterSuffixList.Split(',').ToList();
            }

            #endregion

            _hostConfigList = HostConfigurationBusiness.GetList();

            var zoneList = ZoneBusiness.GetAllList();
            if (null == zoneList || zoneList.Count <= 0) return Task.FromResult(0);

            var waits = new List<EventWaitHandle>();
            var manualEvents =
                new WaitHandle[zoneList.Count];
            var tasks = new List<Task>();

            foreach (var zoneEntity in zoneList)
            {
                try
                {
                    zoneEntity.AuthKey = Utils.AesDecrypt(zoneEntity.AuthKey);
                    tasks.Add(Task.Run(() => StartAnalyze(zoneEntity)));
                }
                catch (Exception ex)
                {
                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.Error, $"Auth key is invalid."));
                }
            }

            Task.WaitAll(tasks.ToArray());

            return Task.FromResult(result: 0);
        }
        

        private Task StartAnalyze(ZoneEntity zoneEntity)
        {
            //var zoneEntity = (ZoneEntity)objZone;
            var dtNow = DateTime.UtcNow;
            var dtStart = dtNow.AddMinutes(-7).AddSeconds(0 - dtNow.Second);
            var dtEnd = dtStart.AddMinutes(2);

            var timeStageList = new List<KeyValuePair<DateTime, DateTime>>();
            while (true)
            {
                timeStageList.Add(new KeyValuePair<DateTime, DateTime>(dtStart, dtStart.AddSeconds(_timeSpan)));

                dtStart = dtStart.AddSeconds(_timeSpan);

                if (dtStart >= dtEnd)
                {
                    break;
                }
            }
            var cloudflare = new CloundFlareApiService(zoneEntity.ZoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);
            var ipWhiteList = cloudflare.GetIpWhitelist();
            var rateLimits = RateLimitBusiness.GetList(zoneEntity.ZoneId).OrderBy(p=>p.OrderNo);

            foreach (var keyValuePair in timeStageList)
            {
                var retryCount = 0;
                dtStart = keyValuePair.Key;
                dtEnd = keyValuePair.Value;

                var timeStage = $"{dtStart:MM/dd/yyyy HH:mm:ss}]-[{dtEnd:MM/dd/yyyy HH:mm:ss}";
                AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App,
                    $"Start getting logs, time range is [{timeStage}]."));

                var cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var retry);

                while (retry && retryCount < 10)
                {
                    retryCount++;
                    cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
                }

                AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, $"Finished getting total [{cloudflareLogs.Count}] records, time range is [{timeStage}]."));

                if (cloudflareLogs.Count > 0)
                {
                    #region 分析日志
                    var requestDetailList = cloudflareLogs.Where(x => !ipWhiteList.Contains(x.ClientIP)).Select(x =>
                    {
                        var model = new LogAnalyzeModel
                        {
                            IP = x.ClientIP,
                            RequestHost = x.ClientRequestHost,
                            RequestFullUrl = $"{x.ClientRequestHost}{x.ClientRequestURI}",
                            RequestUrl =
                                $"{x.ClientRequestHost}{(x.ClientRequestURI.IndexOf('?') > 0 ? x.ClientRequestURI.Substring(0, x.ClientRequestURI.IndexOf('?')) : x.ClientRequestURI)}"
                        };
                        return model;
                    }).Where(x => !IfInSuffixList(x.RequestUrl)).ToList();

                    //AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("除去白名单后日志总计 [{0}] 条", requestDetailList.Count)));

                    if (requestDetailList.Count > 0)
                    {
                        AnalyzeLog(requestDetailList, zoneEntity, rateLimits, timeStage);
                    }
                    #endregion
                }

                #region Remove Ip from blacklist by last trigger time
                var removeLog = RemoveIpFromBlacklistByLastTriggerTime(zoneEntity.ZoneId, dtNow, zoneEntity.IfTestStage, cloudflare, timeStage);
                if (removeLog.Length > 0)
                {
                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.Audit, removeLog));
                }
                #endregion
            }

            return Task.FromResult(result: 0);
        }

        private void AnalyzeLog(List<LogAnalyzeModel> logsAll, ZoneEntity zoneEntity, List<RateLimitEntity> rateLimits, string timeStage)
        {
            var systemLogList = new List<AuditLogEntity>();
            var zoneId = zoneEntity.ZoneId;
            var ifTestStage = zoneEntity.IfTestStage;
            var ifAttacking = false;
            var dtNow = DateTime.UtcNow;
            var banIpLog = string.Empty;
            try
            {
                var cloudflare = new CloundFlareApiService(zoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App,
                    $"Start analyzing logs, time range is [{timeStage}]."));

                #region Analyze log by host access exceed
                var logsIpAll = logsAll.GroupBy(x => new { x.IP, x.RequestHost }).Select(x => new LogAnalyzeModel()
                {
                    IP = x.Key.IP,
                    RequestHost = x.Key.RequestHost,
                    RequestCount = x.Count()
                }).Where(x => IfOverHostRequestLimit(x.RequestHost, x.RequestCount, zoneEntity.ThresholdForHost, zoneEntity.PeriodForHost)
                ).ToList().OrderByDescending(x => x.RequestCount).ThenBy(x => x.RequestHost);

                var logCount = logsIpAll.Count();
                if (zoneEntity.IfAnalyzeByHostRule && logCount > 0)
                {
                    // 发送警报
                    ifAttacking = true;
                    ZoneBusiness.UpdateAttackFlag(true, zoneId);
                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Suspected of an attack, modified the attack token and trigger an alert.", zoneEntity.ZoneName)));

                    var sbDetail = new StringBuilder();
                    sbDetail.Append($"[{logCount}] IPs exceeded the host access threshold, time range is [{timeStage}].<br />");
                    foreach (var rule in logsIpAll)
                    {
                        sbDetail.Append($"IP [{rule.IP}] visited [{rule.RequestHost}] total ({rule.RequestCount} times)]; <br />");
                    }

                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbDetail.ToString()));


                    List<HostConfigurationEntity> currentHostConfigList;
                    foreach (var ipRequestRecord in logsIpAll)
                    {
                        sbDetail = new StringBuilder();
                        currentHostConfigList = _hostConfigList.Where(x => x.Host.Equals(ipRequestRecord.RequestHost)).ToList();
                        sbDetail.AppendFormat("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded host access threshold(Period=[{4}],Threshold=[{5}])，details(only list the top 10 records)：<br />",
                            ipRequestRecord.IP, timeStage, ipRequestRecord.RequestHost, ipRequestRecord.RequestCount,
                            currentHostConfigList.Count > 0 ? currentHostConfigList[0].Period : zoneEntity.PeriodForHost, currentHostConfigList.Count > 0 ? currentHostConfigList[0].Threshold : zoneEntity.ThresholdForHost);

                        var ipRequestList = logsAll.Where(x => x.IP.Equals(ipRequestRecord.IP) && x.RequestHost.Equals(ipRequestRecord.RequestHost))
                            .GroupBy(x => new { x.RequestFullUrl }).Select(x => new LogAnalyzeModel()
                            {
                                RequestFullUrl = x.Key.RequestFullUrl,
                                RequestCount = x.Count()
                            }).ToList().OrderByDescending(x => x.RequestCount).ToList();

                        for (var index = 0; index < Math.Min(ipRequestList.Count(), 10); index++)
                        {
                            sbDetail.AppendFormat("[{0}] {1} times.<br />", ipRequestList[index].RequestFullUrl, ipRequestList[index].RequestCount);
                        }

                        // Ban Ip    
                        banIpLog = BanIpByRateHostConfiguration(zoneEntity, ifTestStage, cloudflare, timeStage, ipRequestRecord, currentHostConfigList);
                     
                        if (!string.IsNullOrEmpty(banIpLog))
                        {
                            sbDetail.Append(banIpLog);
                        }
                        
                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, sbDetail.ToString()));
                    }
                }
                #endregion

                //抽取出所有ratelimit规则中的请求列表
                var logs = logsAll.Where(x => IfInRateLimitRule(x.RequestUrl, rateLimits)).ToList();

                #region Analyze log by rate limit rules
                var ifContainWildcard = false;
                foreach (var rateLimit in rateLimits)
                {
                    //systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App,
                    //    $"Start analyzing rule [Url=[{rateLimit.Url}],Period=[{rateLimit.Period}],Threshold=[{rateLimit.Threshold}],EnlargementFactor=[{rateLimit.EnlargementFactor}]]"));
                    //抽取出所有ratelimit规则中的请求列表
                    ifContainWildcard = rateLimit.Url.EndsWith("*");
                    var logAnalyzeDetailList = ifContainWildcard
                        ? logs.Where(x => x.RequestUrl.ToLower().StartsWith(rateLimit.Url.ToLower().Replace("*", ""))).ToList()
                        : logs.Where(x => x.RequestUrl.ToLower().Equals(rateLimit.Url.ToLower())).ToList();

                    if (logAnalyzeDetailList.Count > 0)
                    {
                        //对IP的请求地址(包含querystring)进行分组
                        var ipRequestListIncludingQueryString = logAnalyzeDetailList.GroupBy(x => new { x.IP, x.RequestFullUrl }).Select(x => new LogAnalyzeModel()
                        {
                            IP = x.Key.IP,
                            RequestFullUrl = x.Key.RequestFullUrl,
                            RequestCount = x.Count()
                        }).ToList();

                        //对IP的请求地址(不包含querystring)进行分组
                        var ipRequestList = logAnalyzeDetailList.GroupBy(x => new { x.IP }).Select(x => new LogAnalyzeModel()
                        {
                            IP = x.Key.IP,
                            RequestUrl = rateLimit.Url,
                            RequestCount = x.Count()
                        }).ToList();

                        //抽取出所有违反规则的IP请求列表
                        var brokenRuleIpList = (from item in ipRequestList
                                                where item.RequestCount / (float)(_timeSpan * _sample) >= (rateLimit.Threshold * rateLimit.EnlargementFactor / (float)rateLimit.Period)
                                                select new LogAnalyzeModel()
                                                {
                                                    IP = item.IP,
                                                    RequestUrl = item.RequestUrl,
                                                    RequestCount = item.RequestCount,
                                                    RateLimitId = rateLimit.ID,
                                                    RateLimitTriggerIpCount = rateLimit.RateLimitTriggerIpCount
                                                }).ToList();

                        var brokenIpCountList = brokenRuleIpList.GroupBy(x => new { x.RateLimitId, x.RequestUrl, x.RateLimitTriggerIpCount }).Select(x => new LogAnalyzeModel()
                        {
                            RateLimitTriggerIpCount = x.Key.RateLimitTriggerIpCount,
                            RateLimitId = x.Key.RateLimitId,
                            RequestUrl = x.Key.RequestUrl,
                            RequestCount = x.Count()
                        }).ToList();

                        //抽取出超过IP数量的规则列表，需要新增或OPEN Cloudflare的Rate Limiting Rule
                        var brokenRuleList = brokenIpCountList.Where(x => x.RateLimitTriggerIpCount <= x.RequestCount).ToList();

                        if (brokenRuleList.Count > 0)
                        {
                            // 发送警报
                            ifAttacking = true;
                            ZoneBusiness.UpdateAttackFlag(true, zoneId);

                            // 更新 Rate Limit Trigger Time
                            RateLimitBusiness.TriggerRateLimit(rateLimit);

                            var sbDetail = new StringBuilder(
                                $"[{brokenRuleIpList.Count}] IPs exceeded rate limiting threshold(Url=[{rateLimit.Url}],Threshold=[{rateLimit.Threshold}],Period=[{rateLimit.Period}],EnlargementFactor=[{rateLimit.EnlargementFactor}]), time range：[{timeStage}], details：<br />");

                            foreach (var rule in brokenRuleIpList)
                            {
                                sbDetail.AppendFormat("IP [{0}] visited [{1}] times.<br /> ", rule.IP, rule.RequestCount);
                            }
                            systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbDetail.ToString()));

                            #region Open Rate Limiting Rule
                            sbDetail = new StringBuilder();
                            //sbDetail.AppendFormat("Start opening rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]].<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                            if (ifTestStage)
                            {
                                sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                            }
                            else
                            {
                                if (cloudflare.OpenRateLimit(rateLimit.Url, rateLimit.Threshold, rateLimit.Period, out var errorLog))
                                {
                                    sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                                }
                                else
                                {
                                    sbDetail.AppendFormat(errorLog.Detail);
                                }
                            }
                            #endregion

                            systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, sbDetail.ToString()));

                            // Ban Ip
                            foreach (var rule in brokenRuleIpList)
                            {
                                sbDetail = new StringBuilder();
                                if (rateLimit.Url.EndsWith("*"))
                                {
                                    sbDetail.Append($"IP [{rule.IP}] visited [{rateLimit.Url}] [{rule.RequestCount}] times, time range：[{timeStage}].<br /> Exceeded rate limiting threshold(URL=[{rateLimit.Url}],Period=[{rateLimit.Period}],Threshold=[{rateLimit.Threshold}],EnlargementFactor=[{rateLimit.EnlargementFactor}])，details(only list the top 10 records)：<br />");

                                    ipRequestList = ipRequestListIncludingQueryString.Where(x => x.IP.Equals(rule.IP))
                                        .OrderByDescending(x => x.RequestCount).ToList();

                                    for (var index = 0; index < Math.Min(ipRequestList.Count(), 10); index++)
                                    {
                                        sbDetail.AppendFormat("[{0}] {1} times.<br />", ipRequestList[index].RequestFullUrl, ipRequestList[index].RequestCount);
                                    }
                                }
                                else
                                {
                                    sbDetail.Append($"IP [{rule.IP}] visited [{rateLimit.Url}] [{rule.RequestCount}] times, time range：[{timeStage}].<br /> Exceeded rate limiting threshold(URL=[{rateLimit.Url}],Period=[{rateLimit.Period}],Threshold=[{rateLimit.Threshold}],EnlargementFactor=[{rateLimit.EnlargementFactor}]).<br />");
                                }
                                banIpLog = BanIpByRateLimitRule(zoneEntity, dtNow, ifTestStage, cloudflare, rateLimit, timeStage, rule);
                                logs.RemoveAll(p => p.IP == rule.IP);
                                if (!string.IsNullOrEmpty(banIpLog)) sbDetail.Append(banIpLog);

                                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, sbDetail.ToString()));
                            }
                        }
                        else
                        {
                            // Remove Cloudflare rate limiting rule by last trigger time
                            var removeRateLimitLog = RemoveCloudflareRateLimitByLastTriggerTime(zoneId, dtNow, ifTestStage, cloudflare, rateLimit);
                            if (null != removeRateLimitLog)
                            {
                                systemLogList.Add(removeRateLimitLog);
                            }
                        }
                    }
                    else
                    {
                        // Remove Cloudflare rate limiting rule by last trigger time
                        var removeRateLimitLog = RemoveCloudflareRateLimitByLastTriggerTime(zoneId, dtNow, ifTestStage, cloudflare, rateLimit);
                        if (null != removeRateLimitLog)
                        {
                            systemLogList.Add(removeRateLimitLog);
                        }
                    }

                    //systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App,
                    //    $"Finished analyzing rate limit rule [Url={rateLimit.Url},Threshold=[{rateLimit.Threshold}],Period=[{rateLimit.Period}],EnlargementFactor=[{rateLimit.EnlargementFactor}]]."));
                }
                #endregion
                
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, $"Finished analyzing cloudflare logs, time range is [{timeStage}]."));
            }
            catch (Exception ex) 
            {
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Error, $"Error in analyzing logs, time range is [{timeStage}], the reason is:[{ex.Message}]. <br />stack trace:{ex.StackTrace}"));
            }
            finally
            {
                if (ifAttacking)
                {
                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit,
                        $"Suspected of an attack in ZoneName [{zoneEntity.ZoneName}], modified the attack token and trigger an alert."));
                }
                else
                {
                    ZoneBusiness.UpdateAttackFlag(false, zoneId); //code review by michael. 记录日志的代码本身报错了怎么办?

                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App,
                        $"There's no attack ,cancel the alert call in ZoneName [{zoneEntity.ZoneName}]."));
                }
                AuditLogBusiness.AddList(systemLogList);
            }
        }

        private string BanIpByRateHostConfiguration(ZoneEntity zoneEntity, bool ifTestStage, CloundFlareApiService cloudflare, string timeStage, LogAnalyzeModel ipRequestRecord, List<HostConfigurationEntity> currentHostConfigList)
        {
            var sbDetail = new StringBuilder();
            if (ifTestStage)
            {
                sbDetail.AppendFormat("Ban IP [{0}] and add ban history successfully.<br />", ipRequestRecord.IP);
                #region 记录BanIp日志
                BanIpHistoryBusiness.Add(new BanIpHistory()
                {
                    IP = ipRequestRecord.IP,
                    ZoneId = zoneEntity.ZoneId,
                    RuleId = 0,
                    Remark = string.Format("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded host access threshold(Period=[{4}],Threshold=[{5}]).",
                ipRequestRecord.IP, timeStage, ipRequestRecord.RequestHost, ipRequestRecord.RequestCount,
                currentHostConfigList.Count > 0 ? currentHostConfigList[0].Period : zoneEntity.PeriodForHost, currentHostConfigList.Count > 0 ? currentHostConfigList[0].Threshold : zoneEntity.ThresholdForHost)
                });
                #endregion
            }
            else
            {
                var cloudflareAccessRuleResponse = cloudflare.BanIp(ipRequestRecord.IP, "Ban Ip By Attack Prevent Windows service!");
                if (cloudflareAccessRuleResponse.Success)
                {
                    sbDetail.AppendFormat("Ban IP [{0}] and add ban history successfully.<br />", ipRequestRecord.IP);
                    #region 记录BanIp日志
                    BanIpHistoryBusiness.Add(new BanIpHistory()
                    {
                        IP = ipRequestRecord.IP,
                        ZoneId = zoneEntity.ZoneId,
                        RuleId = 0,
                        Remark = string.Format("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded host access threshold(Period=[{4}],Threshold=[{5}]).",
                    ipRequestRecord.IP, timeStage, ipRequestRecord.RequestHost, ipRequestRecord.RequestCount,
                    currentHostConfigList.Count > 0 ? currentHostConfigList[0].Period : zoneEntity.PeriodForHost, currentHostConfigList.Count > 0 ? currentHostConfigList[0].Threshold : zoneEntity.ThresholdForHost)
                    });
                    #endregion
                }
                else
                {
                    sbDetail.AppendFormat("Ban IP [{0}] failure, the reason is：[{1}].<br />", ipRequestRecord.IP, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "Cloudflare does not return any error message");
                }
            }
            return sbDetail.ToString();
        }

        private string BanIpByRateLimitRule(ZoneEntity zoneEntity, DateTime dtNow, bool ifTestStage, CloundFlareApiService cloudflare, RateLimitEntity rateLimit, string timeStage, LogAnalyzeModel logAnalyzeModel)
        {
            var sbDetail = new StringBuilder();
            if (ifTestStage)
            {
                sbDetail.AppendFormat("Ban IP [{0}] and add ban history successfully.<br />", logAnalyzeModel.IP);
                #region 记录BanIp日志
                BanIpHistoryBusiness.Add(new BanIpHistory()
                {
                    IP = logAnalyzeModel.IP,
                    ZoneId = zoneEntity.ZoneId,
                    RuleId = 0,
                    Remark = string.Format("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded rate limit threshold(Period=[{4}],Threshold=[{5}]).",
                logAnalyzeModel.IP, timeStage, logAnalyzeModel.RequestUrl, logAnalyzeModel.RequestCount, rateLimit.Period, rateLimit.Threshold)
                });
                #endregion
            }
            else
            {
                var cloudflareAccessRuleResponse = cloudflare.BanIp(logAnalyzeModel.IP, "Ban Ip By Attack Prevent Windows service!");
                if (cloudflareAccessRuleResponse.Success)
                {
                    sbDetail.AppendFormat("Ban IP [{0}] and add ban history successfully.<br />", logAnalyzeModel.IP);
                    #region 记录BanIp日志
                    BanIpHistoryBusiness.Add(new BanIpHistory()
                    {
                        IP = logAnalyzeModel.IP,
                        ZoneId = zoneEntity.ZoneId,
                        RuleId = 0,
                        Remark = string.Format("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded rate limit threshold(Period=[{4}],Threshold=[{5}]).",
                        logAnalyzeModel.IP, timeStage, logAnalyzeModel.RequestHost, logAnalyzeModel.RequestCount, rateLimit.Period, rateLimit.Threshold)
                    });
                    #endregion
                }
                else
                {
                    sbDetail.AppendFormat("Ban IP [{0}] failure, the reason is：[{1}].<br />", logAnalyzeModel.IP, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "No error message from Cloudflare.");
                }
            }
            return sbDetail.ToString();
        }

        private AuditLogEntity RemoveCloudflareRateLimitByLastTriggerTime(string zoneId, DateTime dtNow, bool ifTestStage, CloundFlareApiService cloudflare, RateLimitEntity rateLimit)
        {
            AuditLogEntity log = null;
            //没有触犯该条Rate Limit，检查是否需要关闭Rate Limit
            if (!((dtNow - rateLimit.LatestTriggerTime).TotalHours > rateLimit.RateLimitTriggerTime)) return null;
            var rule = cloudflare.GetRateLimitRule(rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
            if (null == rule) return null;
            if (ifTestStage)
            {
                log = new AuditLogEntity(zoneId, LogLevel.Audit,
                    $"No Ip broke the rate limit rule [Url=[{rateLimit.Url}],Threshold=[{rateLimit.Threshold}],Period=[{rateLimit.Period}]], last trigger time is [{rateLimit.LatestTriggerTime}], remove the rule successfully.");
            }
            else
            {
                //如果距离上次触发时间已经超过配置的RateLimitTriggerTime，则关闭该rate limit
                var response = cloudflare.DeleteRateLimit(rule.Id);
                if (response.success)
                {
                    log = new AuditLogEntity(zoneId, LogLevel.Audit,
                        $"No Ip broke the rate limit rule [Url=[{rateLimit.Url}],Threshold=[{rateLimit.Threshold}],Period=[{rateLimit.Period}]], last trigger time is [{rateLimit.LatestTriggerTime}], remove the rule successfully.");
                }
                else
                {
                    log = new AuditLogEntity(zoneId, LogLevel.Error,
                        $"No Ip broke the rate limit rule [Url =[{rateLimit.Url}],Threshold =[{rateLimit.Threshold}],Period =[{rateLimit.Period}]], last trigger time is [{rateLimit.LatestTriggerTime}], remove the rule failure, the reason is:[{(response.errors.Any() ? response.errors[0].message : "No error message from Cloudflare.")}].");
                }
            }

            return log;
        }

        private string RemoveIpFromBlacklistByLastTriggerTime(string zoneId, DateTime dtNow, bool ifTestStage, CloundFlareApiService cloudflare, string timeStage)
        {
            CloudflareAccessRuleResponse cloudflareAccessRuleResponse = null;
            //如果黑名单中的Ip在CancelBanIpTime时间内没有触发规则，则删除黑名单
            var banIpHistories = BanIpHistoryBusiness.Get(zoneId);
            var sbCancelBanLog = new StringBuilder();
            foreach (var banIpHistory in banIpHistories)
            {
                if (!string.IsNullOrEmpty(banIpHistory.IP))
                {
                    if ((dtNow - banIpHistory.LatestTriggerTime).TotalHours > _cancelBanIpTime)
                    {
                        if (ifTestStage)
                        {
                            sbCancelBanLog.AppendFormat("Remove IP [{0}] from blacklist successfully, last trigger time is [{1}].<br />", banIpHistory.IP, banIpHistory.LatestTriggerTime);
                        }
                        else
                        {
                            cloudflareAccessRuleResponse = cloudflare.RemoveIpFromBlacklist(banIpHistory.IP);
                            if (cloudflareAccessRuleResponse.Success)
                            {
                                if (BanIpHistoryBusiness.Delete(zoneId, banIpHistory.Id))
                                {
                                    sbCancelBanLog.AppendFormat("Remove IP [{0}] from blacklist successfully, last trigger time is [{1}].<br />", banIpHistory.IP, banIpHistory.LatestTriggerTime);
                                }
                            }
                            else
                            {
                                sbCancelBanLog.AppendFormat("Remove IP [{0}] from blacklist failure, the reason is：[{1}].<br />", banIpHistory.IP, cloudflareAccessRuleResponse.Errors.Any() ? cloudflareAccessRuleResponse.Errors[0] : "No error message from Cloudflare.");
                            }
                        }
                    }
                }
            }
            return sbCancelBanLog.ToString();
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

        private bool IfInRateLimitRule(string requestUrl, List<RateLimitEntity> rateLimits)
        {
            if (null != rateLimits)
            {
                foreach (var rateLimit in rateLimits)
                {
                    if (rateLimit.Url.EndsWith("*"))
                    {
                        if (requestUrl.ToLower().StartsWith(rateLimit.Url.Replace("*", "").ToLower()))
                            return true;
                    }
                    else
                    {
                        if (requestUrl.ToLower().Equals(rateLimit.Url.ToLower()))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool IfOverHostRequestLimit(string hostStr, int requestCount, int globalThreshold, int globalPeriod)
        {
            foreach (var hostConfig in _hostConfigList)
            {
                // code review by page 这个地方用==全字匹配是否合适
                if (hostConfig.Host.Equals(hostStr))
                {
                    return requestCount / (float)(_timeSpan * _sample) >= ((float)hostConfig.Threshold / hostConfig.Period);
                }
            }
            //这段代码意图是？
            return requestCount / (float)(_timeSpan * _sample) >= ((float)globalThreshold / globalPeriod);
        }
    }
}
