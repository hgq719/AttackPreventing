﻿using AttackPrevent.Business;
using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AttackPrevent.WindowsService.Job
{
    public class LogAnalyzeJob 
    {
        private double _sample = 1;
        private int _timeSpan = 60;
        private int _cancelBanIpTime = 120;
        private int _cancelAttackTime = 5;
        private readonly ILogService _logService = new LogService();


        private List<string> _suffixList = new List<string>
        {
            "bmp","ejs","jpeg","pdf","ps","ttf","class","eot","jpg","pict"
            ,"svg","webp","css","eps","js","pls","svgz","woff","csv","gif"
            ,"mid","png","swf","woff2","doc","ico","midi","ppt","tif","xls"
            ,"docx","jar","otf","pptx","tiff","xlsx"
        };

        public LogAnalyzeJob()
        {
        }

        public void Execute()
        {
            //Set global paramter
            var globalConfigurations = GlobalConfigurationBusiness.GetConfigurationList();
            if (null != globalConfigurations && globalConfigurations.Count > 0)
            {
                _sample = globalConfigurations[index: 0].GlobalSample;
#if DEBUG
                _timeSpan = 5;
#else
                _timeSpan = globalConfigurations[index: 0].GlobalTimeSpan;
#endif
                _timeSpan = globalConfigurations[index: 0].GlobalTimeSpan;
                _cancelBanIpTime = globalConfigurations[index: 0].CancelBanIPTime;
                _cancelAttackTime = globalConfigurations[index: 0].CancelAttackTime;
            }

            var filterSuffixList = ConfigurationManager.AppSettings["FilterSuffixList"];
            if (!string.IsNullOrEmpty(filterSuffixList))
            {
                _suffixList = filterSuffixList.Split(',').ToList();
            }
            
            var zoneList = ZoneBusiness.GetAllList();
            if(zoneList.Count == 0) { return;}

            ZoneLockerManager.RefreshZoneLockers(zoneList);
            foreach (var zoneEntity in zoneList)
            {
                try
                {
                    if (zoneEntity.IfEnable)
                    {
                        var rateLimitingCount = RateLimitBusiness.GetList(zoneEntity.ZoneId).Count();
                        if (rateLimitingCount > 0)
                        {
                            zoneEntity.AuthKey = Utils.AesDecrypt(zoneEntity.AuthKey);
#if DEBUG
                            StartAnalyze(zoneEntity);
#else
                            var task = new Task(() => { StartAnalyze(zoneEntity); });
                            task.Start();
#endif
                        }
                    }
                }
                catch (Exception ex)
                {
                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Error, $"Auth key is invalid."));
                    _logService.Error($" error message = {ex.Message}. \n stack trace = {ex.StackTrace}");
                }
            }
        }
        
        private void StartAnalyze(ZoneEntity zoneEntity)
        {
            Console.WriteLine($"{DateTime.UtcNow} Start to analyze zone: {zoneEntity.ZoneName}");
            var sw = new Stopwatch();
            sw.Start();

            var zoneId = zoneEntity.ZoneId;
            var systemLogList = new List<AuditLogEntity>();
            try
            {
#if DEBUG
                //if (ZoneLockerManager.IsRunning(zoneId))
                //{
                //    return;
                //}
                //else
                //{
                //    ZoneLockerManager.SetZoneRunningStatus(zoneId, true);
                //}
#else
                if (ZoneLockerManager.IsRunning(zoneId))
                {
                    return;
                }
                else
                {
                    ZoneLockerManager.SetZoneRunningStatus(zoneId, true);
                }
#endif
                var dtNow = DateTime.UtcNow;
#if DEBUG
                var dtStart = dtNow.AddMinutes(-120).AddSeconds(0 - dtNow.Second);
                var dtEnd = dtStart.AddSeconds(60);
#else
                var dtStart = dtNow.AddMinutes(-3).AddSeconds(0 - dtNow.Second);
                var dtEnd = dtStart.AddMinutes(2);
#endif

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
                var cloudflare = new CloudFlareApiService(zoneEntity.ZoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);

                var ipWhiteList = cloudflare.GetIpWhitelist(out var errorLog);
                if (!string.IsNullOrEmpty(errorLog))
                {
                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Error, errorLog));
                }

#region 如果接口中取不到白名单，则从文件中获取
                var zoneWhiteListCache = ZoneWhiteListCache.GetInstance();
                if (ipWhiteList.Count > 0)
                {
                    if (zoneWhiteListCache.AddZoneWhiteList(zoneId, ipWhiteList))
                    {
                        AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                            $"Update whitelist cache successfully."));
                    }
                    else
                    {
                        AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Error,
                            $"Update whitelist cache failure."));
                    }
                }
                else
                {
                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Error,
                        $"Call whitelist api failure from cloudflare."));
                    //接口错误，返回不了白名单，则直接从文件中取
                    ipWhiteList = zoneWhiteListCache.GetZoneWhiteList(zoneId);
                    if (ipWhiteList.Count > 0)
                    {
                        AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                            $"Get whitelist from cache."));
                    }
                    else
                    {
                        AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Error,
                            $"There's no data in whitelist cache."));
                    }
                }
#endregion
                
                var rateLimits = RateLimitBusiness.GetList(zoneEntity.ZoneId).OrderBy(p => p.OrderNo).ToList();

                foreach (var keyValuePair in timeStageList)
                {
                    var retryCount = 0;
                    dtStart = keyValuePair.Key;
                    dtEnd = keyValuePair.Value;

                    var timeStage = $"{dtStart:MM/dd/yyyy HH:mm:ss}]-[{dtEnd:MM/dd/yyyy HH:mm:ss}";
                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                        $"Start getting logs, time range is [{timeStage}]."));


#if DEBUG
                    var cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 0.1, out var
                        retry);
#else
                    var cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var
                        retry);
#endif

                    while (retry && retryCount < 10)
                    {
                        retryCount++;
                        cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
                    }

                    AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                        $"Finished getting total [{cloudflareLogs.Count}] records, start to analyze logs, the time range is [{timeStage}]."));

                    if (cloudflareLogs.Count > 0)
                    {
                        var whiteListLogs = cloudflareLogs.Where(x => ipWhiteList.Contains(x.ClientIP)).ToList();

                        AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                            $"Getting total [{ipWhiteList.Count}] ips whitelist and total [{whiteListLogs.Count}] records logs for white list, the time range is [{timeStage}]."));

                        var requestDetailList = cloudflareLogs.Where(x => !ipWhiteList.Contains(x.ClientIP)).Select(x =>
                        {
                            var model = new LogAnalyzeModel
                            {
                                IP = x.ClientIP,
                                RequestHost = x.ClientRequestHost,
                                RequestFullUrl = $"{RemovePortFromHost(x.ClientRequestHost)}{x.ClientRequestURI}",
                                RequestUrl =
                                    $"{RemovePortFromHost(x.ClientRequestHost)}{(x.ClientRequestURI.IndexOf('?') > 0 ? x.ClientRequestURI.Substring(0, x.ClientRequestURI.IndexOf('?')) : x.ClientRequestURI)}"
                            };
                            return model;
                        }).Where(x => !IfInSuffixList(x.RequestUrl)).ToList();

                        if (requestDetailList.Count > 0)
                        {
                            AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                                $"Finished getting total [{requestDetailList.Count}] records after filtering white list and static file, start to analyze logs, the time range is [{timeStage}]."));
                            AnalyzeLog(requestDetailList, zoneEntity, rateLimits, timeStage);
                        }
                        else
                        {
                            foreach (var rateLimit in rateLimits)
                            {
                                // Remove Cloudflare rate limiting rule by last trigger time
                                var removeRateLimitLog = RemoveCloudflareRateLimitByLastTriggerTime(zoneEntity.TableID, dtNow, zoneEntity.IfTestStage, cloudflare, rateLimit);
                                if (null != removeRateLimitLog)
                                {
                                    systemLogList.Add(removeRateLimitLog);
                                }
                            }
                            //获取当前ZoneId上一次攻击的时间，如果时间大于配置的时间M（Min），则关闭攻击标志
                            if (ZoneBusiness.CancelAttack(_cancelAttackTime, zoneEntity.ZoneId))
                            {
                                systemLogList.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                                    $"There's no attack more than {_cancelAttackTime} minutes, cancel the alert call in ZoneName [{zoneEntity.ZoneName}], the time range is [{timeStage}]."));
                            }
                        }
                    }
                    else
                    {
                        foreach (var rateLimit in rateLimits)
                        {
                            // Remove Cloudflare rate limiting rule by last trigger time
                            var removeRateLimitLog = RemoveCloudflareRateLimitByLastTriggerTime(zoneEntity.TableID, dtNow, zoneEntity.IfTestStage, cloudflare, rateLimit);
                            if (null != removeRateLimitLog)
                            {
                                systemLogList.Add(removeRateLimitLog);
                            }
                        }
                        //获取当前ZoneId上一次攻击的时间，如果时间大于配置的时间M（Min），则关闭攻击标志
                        if (ZoneBusiness.CancelAttack(_cancelAttackTime, zoneEntity.ZoneId))
                        {
                            systemLogList.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.App,
                                $"There's no attack more than {_cancelAttackTime} minutes, cancel the alert call in ZoneName [{zoneEntity.ZoneName}], the time range is [{timeStage}]."));
                        }
                    }

                    if (systemLogList.Count > 0)
                    {
                        AuditLogBusiness.AddList(systemLogList);
                    }

                    var removeLog = RemoveIpFromBlacklistByLastTriggerTime(zoneEntity.ZoneId, dtNow,
                        zoneEntity.IfTestStage, cloudflare, timeStage);
                    if (removeLog.Length > 0)
                    {
                        AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Audit, removeLog));
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = $" Analyse Zone log failure, error message = {ex.Message}. \n stack trace = {ex.StackTrace}";
                AuditLogBusiness.Add(new AuditLogEntity(zoneEntity.TableID, LogLevel.Error, msg));
            }
            finally
            {
                ZoneLockerManager.SetZoneRunningStatus(zoneId, false);
                sw.Stop();
                Console.WriteLine($"{DateTime.UtcNow} end to analyze zone: {zoneEntity.ZoneName},elapsed time : {sw.ElapsedMilliseconds/1000}s");
            }
        }

        private string RemovePortFromHost(string host)
        {
            var result = host;
            try
            {
                var portIndex = host.Replace("：",":").IndexOf(":");
                if (portIndex > 0)
                {
                    result = host.Substring(0, portIndex);
                }

                return result;
            }
            catch (Exception)
            {
                return host;
            }
        }

        private void AnalyzeLog(IEnumerable<LogAnalyzeModel> logsAll, ZoneEntity zoneEntity, List<RateLimitEntity> rateLimits, string timeStage)
        {
            var systemLogList = new List<AuditLogEntity>();
            var zoneTableId = zoneEntity.TableID;
            var ifTestStage = zoneEntity.IfTestStage;
            var ifAttacking = false;
            var dtNow = DateTime.UtcNow;
            string banIpLog;
            try
            {
                var cloudflare = new CloudFlareApiService(zoneEntity.ZoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);

                //抽取出所有ratelimit规则中的请求列表
                var logs = logsAll.Where(x => IfInRateLimitRule(x.RequestUrl, rateLimits)).ToList();

#region Analyze log by rate limit rules
                bool ifContainWildcard = false;
                var rateLimitIndex = 0;
                foreach (var rateLimit in rateLimits)
                {
                    rateLimitIndex++;
                    //抽取出所有ratelimit规则中的请求列表
                    rateLimit.Url = rateLimit.Url.Trim();
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
                            if (rateLimit.IfTesting)
                            {
                                systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Audit, "[Testing Prompts]:Update attack flag and  trigger time while attacking"));
                            }
                            else
                            {
                                // 发送警报
                                ifAttacking = true;
                                if (ZoneBusiness.UpdateAttackFlag(true, zoneEntity.ZoneId))
                                {
                                    systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Audit, "Update attack flag and  trigger time while attacking"));
                                }
                            }

                            // 更新 Rate Limit Trigger Time
                            RateLimitBusiness.TriggerRateLimit(rateLimit);

                            var sbDetail = new StringBuilder(
                                $"[{brokenRuleIpList.Count}] IPs exceeded rate limiting threshold(Url=[{rateLimit.Url}],Threshold=[{rateLimit.Threshold}],Period=[{rateLimit.Period}],EnlargementFactor=[{rateLimit.EnlargementFactor}]), time range：[{timeStage}], details：<br />");

                            foreach (var rule in brokenRuleIpList)
                            {
                                sbDetail.AppendFormat("IP [{0}] visited [{1}] times.<br /> ", rule.IP, rule.RequestCount);
                            }
                            systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.App, sbDetail.ToString()));

#region Open Rate Limiting Rule
                            sbDetail = new StringBuilder();
                            //sbDetail.AppendFormat("Start opening rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]].<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                            if (ifTestStage || rateLimit.IfTesting)
                            {
                                sbDetail.AppendFormat("[Testing Prompts]:Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                            }
                            else if(rateLimit.IfOpenRateLimitRule)
                            {
                                if (cloudflare.OpenRateLimit(rateLimit.Url, rateLimit.Threshold, rateLimit.Period, out var errorLog))
                                {
                                    sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                                }
                                else
                                {
                                    systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Error, errorLog.Detail));
                                }
                            }
#endregion

                            systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Audit, sbDetail.ToString()));

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
                                banIpLog = BanIpByRateLimitRule(zoneEntity, ifTestStage, cloudflare, rateLimit, timeStage, rule, out var errorLog);
                                //logs.RemoveAll(p => p.IP == rule.IP);
                                if (!string.IsNullOrEmpty(banIpLog)) sbDetail.Append(banIpLog);

                                systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Audit, sbDetail.ToString()));
                                if (!string.IsNullOrEmpty(errorLog))
                                {
                                    systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Error, sbDetail.ToString()));
                                }
                            }
                        }
                        else
                        {
                            // Remove Cloudflare rate limiting rule by last trigger time
                            var removeRateLimitLog = RemoveCloudflareRateLimitByLastTriggerTime(zoneTableId, dtNow, ifTestStage, cloudflare, rateLimit);
                            if (null != removeRateLimitLog)
                            {
                                systemLogList.Add(removeRateLimitLog);
                            }
                        }
                    }
                    else
                    {
                        // Remove Cloudflare rate limiting rule by last trigger time
                        var removeRateLimitLog = RemoveCloudflareRateLimitByLastTriggerTime(zoneTableId, dtNow, ifTestStage, cloudflare, rateLimit);
                        if (null != removeRateLimitLog)
                        {
                            systemLogList.Add(removeRateLimitLog);
                        }
                    }

                    
                    if(IsLastRateLimitRuleWithSameUrl(rateLimits, rateLimit, rateLimitIndex))
                    {
                        //判断当前url是否属于所有ratelimit中同一url多条规则中的最后一条
                        //删除当前ratelimit的所有url
                        logs.RemoveAll(x => ifContainWildcard ? x.RequestUrl.ToLower().StartsWith(rateLimit.Url.ToLower().Replace("*", "")) : x.RequestUrl.ToLower().Equals(rateLimit.Url.ToLower()));
                        systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Audit, $"Current rule [URL=[{0}],Threshold=[{1}],Period=[{2}]] is last rule of url[{rateLimit.Url}] in all rate limit rules, remove all logs of current url from analyzing logs successfully.<br />"));
                    }
                }
#endregion
                
                systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.App, $"Finished analyzing cloudflare logs, time range is [{timeStage}]."));
            }
            catch (Exception ex) 
            {
                systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Error, $"Error in analyzing logs, time range is [{timeStage}], the reason is:[{ex.Message}]. <br />stack trace:{ex.StackTrace}"));
            }
            finally
            {
                if (ifAttacking)
                {
                    systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.Audit,
                        $"Suspected of an attack in ZoneName [{zoneEntity.ZoneName}], modified the attack token and trigger an alert, time range：[{timeStage}]."));
                }
                else
                {
                    //获取当前ZoneId上一次攻击的时间，如果时间大于配置的时间M（Min），则关闭攻击标志
                    if (ZoneBusiness.CancelAttack(_cancelAttackTime, zoneEntity.ZoneId))
                    {
                        systemLogList.Add(new AuditLogEntity(zoneTableId, LogLevel.App,
                            $"There's no attack more than {_cancelAttackTime} minutes, cancel the alert call in ZoneName [{zoneEntity.ZoneName}]."));
                    }
                }
                AuditLogBusiness.AddList(systemLogList);
            }
        }

        /// <summary>
        /// 判断当前规则是否是属于所有规则里面，当前url所对应的最后一条规则
        /// </summary>
        /// <param name="rateLimits"></param>
        /// <param name="rateLimit"></param>
        /// <param name="rateLimitIndex"></param>
        /// <returns></returns>
        private bool IsLastRateLimitRuleWithSameUrl(List<RateLimitEntity> rateLimits, RateLimitEntity rateLimit, int rateLimitIndex)
        {
            var isLastRuleWithSameUrl = true;
            for (int i = rateLimitIndex; i < rateLimits.Count; i++)
            {
                if (rateLimits[i].Url.ToLower() == rateLimit.Url.ToLower())
                {
                    isLastRuleWithSameUrl = false;
                }
            }

            return isLastRuleWithSameUrl;
        }

        private string BanIpByRateLimitRule(ZoneEntity zoneEntity, bool ifTestStage, CloudFlareApiService cloudflare, RateLimitEntity rateLimit, string timeStage, LogAnalyzeModel logAnalyzeModel, out string errorLog)
        {
            errorLog = string.Empty;
            var sbDetail = new StringBuilder();
            if (ifTestStage || rateLimit.IfTesting)
            {
                sbDetail.AppendFormat("[Testing Prompts]:Ban IP [{0}] successfully.<br />", logAnalyzeModel.IP);
            }
            else if(rateLimit.IfBanIp)
            {
                var cloudflareAccessRuleResponse = cloudflare.BanIp(logAnalyzeModel.IP, "Ban Ip By Attack Prevent Windows service!");
                if (cloudflareAccessRuleResponse.Success)
                {
                    sbDetail.AppendFormat("Ban IP [{0}] and add ban history successfully.<br />", logAnalyzeModel.IP);
                    
                    BanIpHistoryBusiness.Add(new BanIpHistory()
                    {
                        IP = logAnalyzeModel.IP,
                        ZoneId = zoneEntity.ZoneId,
                        RuleId = 0,
                        Remark = string.Format("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded rate limit threshold(Period=[{4}],Threshold=[{5}]).",
                        logAnalyzeModel.IP, timeStage, logAnalyzeModel.RequestHost, logAnalyzeModel.RequestCount, rateLimit.Period, rateLimit.Threshold)
                    });
                }
                else
                {
                    errorLog = string.Format("Ban IP [{0}] failure, the reason is：[{1}].<br />", logAnalyzeModel.IP,
                        cloudflareAccessRuleResponse.Errors.Length > 0
                            ? cloudflareAccessRuleResponse.Errors[0].message
                            : "No error message from Cloudflare.");
                }
            }
            return sbDetail.ToString();
        }

        private AuditLogEntity RemoveCloudflareRateLimitByLastTriggerTime(int zoneTableId, DateTime dtNow, bool ifTestStage, CloudFlareApiService cloudflare, RateLimitEntity rateLimit)
        {
            AuditLogEntity log;
            //没有触犯该条Rate Limit，检查是否需要关闭Rate Limit
            if (!((dtNow - rateLimit.LatestTriggerTime).TotalHours > rateLimit.RateLimitTriggerTime)) return null;
            var rule = cloudflare.GetRateLimitRule(rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
            if (null == rule) return null;
            if (ifTestStage || rateLimit.IfTesting)
            {
                log = new AuditLogEntity(zoneTableId, LogLevel.Audit,
                    $"[Testing Prompts]:No Ip broke the rate limit rule [Url=[{rateLimit.Url}],Threshold=[{rateLimit.Threshold}],Period=[{rateLimit.Period}]], last trigger time is [{rateLimit.LatestTriggerTime}], remove the rule successfully.");
            }
            else
            {
                //如果距离上次触发时间已经超过配置的RateLimitTriggerTime，则关闭该rate limit
                var response = cloudflare.DeleteRateLimit(rule.Id);
                if (response.success)
                {
                    log = new AuditLogEntity(zoneTableId, LogLevel.Audit,
                        $"No Ip broke the rate limit rule [Url=[{rateLimit.Url}],Threshold=[{rateLimit.Threshold}]" + $",Period=[{rateLimit.Period}]], last trigger time is [{rateLimit.LatestTriggerTime}], remove the rule successfully.");
                }
                else
                {
                    log = new AuditLogEntity(zoneTableId, LogLevel.Error,
                        $"No Ip broke the rate limit rule [Url =[{rateLimit.Url}],Threshold =[{rateLimit.Threshold}]" + $",Period =[{rateLimit.Period}]], last trigger time is [{rateLimit.LatestTriggerTime}], remove the rule failure, " + $"the reason is:[{(response.errors.Length > 0 ? response.errors[0].message : "No error message from Cloudflare.")}].");
                }
            }

            return log;
        }

        private string RemoveIpFromBlacklistByLastTriggerTime(string zoneId, DateTime dtNow, bool ifTestStage, CloudFlareApiService cloudflare, string timeStage)
        {
            if (timeStage == null) throw new ArgumentNullException(nameof(timeStage));
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
                                sbCancelBanLog.AppendFormat("Remove IP [{0}] from blacklist failure, the reason is：[{1}].<br />", banIpHistory.IP, cloudflareAccessRuleResponse.Errors.Any() ? cloudflareAccessRuleResponse.Errors[0].message : "No error message from Cloudflare.");
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

    }
}
