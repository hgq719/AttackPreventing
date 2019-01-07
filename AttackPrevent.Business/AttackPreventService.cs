using AttackPrevent.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface IAttackPreventService
    {
        void Add(AnalyzeResult analyzeResult);
        void doWork();
    }
    public class AttackPreventService : IAttackPreventService
    {
        private static IAttackPreventService attackPreventService;
        private static object obj_Sync = new object();
        private ConcurrentQueue<AnalyzeResult> analyzeResults;
        private ILogService logger = new LogService();
        private AttackPreventService()
        {
            analyzeResults = new ConcurrentQueue<AnalyzeResult>();
        }
        public static IAttackPreventService GetInstance()
        {
            if (attackPreventService == null)
            {
                lock (obj_Sync)
                {
                    if (attackPreventService == null)
                    {
                        attackPreventService = new AttackPreventService();
                    }
                }
            }
            return attackPreventService;
        }

        public void Add(AnalyzeResult analyzeResult)
        {
            //analyzeResults.Enqueue(analyzeResult);
            if ( analyzeResult != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Analyze(analyzeResult);
                stopwatch.Stop();
                logger.Debug(stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        public void doWork()
        {
            while (true)
            {
                AnalyzeResult analyzeResult = null;
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    if (analyzeResults.TryDequeue(out analyzeResult))
                    {
                        Analyze(analyzeResult);
                        stopwatch.Stop();
                        logger.Debug(stopwatch.Elapsed.TotalMilliseconds);
                    }

                }
                catch(Exception e)
                {
                    logger.Error(e.StackTrace);
                    if (analyzeResult != null)
                    {
                        Add(analyzeResult);
                    }
                }
                finally
                {

                }
            }
        }
        private void Analyze(AnalyzeResult analyzeResult)
        {
            if (analyzeResult != null && analyzeResult.result != null)
            {
                List<AuditLogEntity> auditLogEntities = new List<AuditLogEntity>();
                string key = "AnalyzeRatelimit_GetZoneList_Key";
                List<ZoneEntity> zoneList = Utils.GetMemoryCache(key, () =>
                {
                    return ZoneBusiness.GetZoneList();
                }, 1440);

                string zoneID = analyzeResult.ZoneId;
                var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);

                if (zone != null)
                {
                    string authEmail = zone.AuthEmail;
                    string authKey = zone.AuthKey;

                    var cloudflare = new CloudFlareApiService(zone.ZoneId, zone.AuthEmail, zone.AuthKey);

                    //发送警报
                    Warn(analyzeResult);

                    //开启RateLimit
                    var logs = OpenRageLimit(zone, cloudflare, analyzeResult);
                    auditLogEntities.AddRange(logs);

                    //Ban IP
                    logs = BanIp(zone, cloudflare, analyzeResult);
                    auditLogEntities.AddRange(logs);

                    //记录日志
                    InsertLogs(logs);
                }

            }
        }
        private void InsertLogs(List<AuditLogEntity> logs)
        {
            if (logs != null)
            {
                IISLogBusiness.AddList(logs);
            }
        }
        private List<AuditLogEntity> OpenRageLimit(ZoneEntity zone, CloudFlareApiService cloudflare, AnalyzeResult analyzeResult)
        {
            List<AuditLogEntity> auditLogEntities = new List<AuditLogEntity>();
            #region Open Rate Limiting Rule
            
            foreach (var rst in analyzeResult.result)
            {
                StringBuilder sbDetail = new StringBuilder();
                //sbDetail.AppendFormat("Start opening rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]].<br />", rateLimit.Url, rateLimit.Threshold, rateLimit.Period);
                if (zone != null && zone.IfTestStage)
                {
                    sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rst.Url, rst.Threshold, rst.Period);
                }
                else
                {
                    if (cloudflare.OpenRateLimit(rst.Url, rst.Threshold, rst.Period, out var errorLog))
                    {
                        sbDetail.AppendFormat("Open rate limiting rule in Cloudflare [URL=[{0}],Threshold=[{1}],Period=[{2}]] successfully.<br />", rst.Url, rst.Threshold, rst.Period);
                    }
                    else
                    {
                        sbDetail.AppendFormat(errorLog.Detail);
                    }
                }

                auditLogEntities.Add(new AuditLogEntity(zone.TableID, LogLevel.Audit, sbDetail.ToString()));
            }

            #endregion
            return auditLogEntities;
        }
        private List<AuditLogEntity> BanIp(ZoneEntity zone, CloudFlareApiService cloudflare, AnalyzeResult analyzeResult)
        {
            List<AuditLogEntity> auditLogEntities = new List<AuditLogEntity>();

            foreach (var rst in analyzeResult.result)
            {
                foreach (var broken in rst.BrokenIpList)
                {
                    StringBuilder sbDetail = new StringBuilder();
                    if (rst.Url.EndsWith("*"))
                    {
                        sbDetail.Append($"IP [{broken.IP}] visited [{rst.Url}] [{broken.RequestRecords.Count}] times, time range：[{analyzeResult.timeStage}].<br /> Exceeded rate limiting threshold(URL=[{rst.Url}],Period=[{rst.Period}],Threshold=[{rst.Threshold}],EnlargementFactor=[{rst.EnlargementFactor}])，details(only list the top 10 records)：<br />");
                        var top10 = broken.RequestRecords.OrderByDescending(a => a.RequestCount).Take(10);
         
                        foreach (var item in top10)
                        {
                            sbDetail.AppendFormat("[{0}] {1} times.<br />", item.FullUrl, item.RequestCount);
                        }                       
                    }
                    else
                    {
                        sbDetail.Append($"IP [{broken.IP}] visited [{rst.Url}] [{broken.RequestRecords.Count}] times, time range：[{analyzeResult.timeStage}].<br /> Exceeded rate limiting threshold(URL=[{rst.Url}],Period=[{rst.Period}],Threshold=[{rst.Threshold}],EnlargementFactor=[{rst.EnlargementFactor}]).<br />");
                    }
                    string banIpLog = BanIpByRateLimitRule(zone, zone.IfTestStage, cloudflare, analyzeResult.timeStage ,broken.RequestRecords.FirstOrDefault().HostName, broken.IP,rst.Period,rst.Threshold,rst.BrokenIpList.Count);

                    if (!string.IsNullOrEmpty(banIpLog)) sbDetail.Append(banIpLog);

                    auditLogEntities.Add(new AuditLogEntity(zone.TableID, LogLevel.Audit, sbDetail.ToString()));

                }

            }

            return auditLogEntities;
        }
        private void Warn(AnalyzeResult analyzeResult)
        {
            // 发送警报
            ZoneBusiness.UpdateAttackFlag(true, analyzeResult.ZoneId);
        }
        private string BanIpByRateLimitRule(ZoneEntity zoneEntity, bool ifTestStage, CloudFlareApiService cloudflare, int timeStage, string requestHost, string ip, int period, int threshold, int requestCount)
        {
            var sbDetail = new StringBuilder();
            if (ifTestStage)
            {
                sbDetail.AppendFormat("Ban IP [{0}] successfully.<br />", ip);
            }
            else
            {
                var cloudflareAccessRuleResponse = cloudflare.BanIp(ip, "Ban Ip By Attack Prevent Windows service!");
                if (cloudflareAccessRuleResponse.Success)
                {
                    sbDetail.AppendFormat("Ban IP [{0}] and add ban history successfully.<br />", ip);

                    BanIpHistoryBusiness.Add(new BanIpHistory()
                    {
                        IP = ip,
                        ZoneId = zoneEntity.ZoneId,
                        RuleId = 0,
                        Remark = string.Format("IP [{0}] visited [{2}] [{3}] times, time range：[{1}].<br /> Exceeded rate limit threshold(Period=[{4}],Threshold=[{5}]).",
                        ip, timeStage, requestHost, requestCount, period, threshold)
                    });
                }
                else
                {
                    sbDetail.AppendFormat("Ban IP [{0}] failure, the reason is：[{1}].<br />", ip, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "No error message from Cloudflare.");
                }
            }
            return sbDetail.ToString();
        }
    }
}
