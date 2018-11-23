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
        //private ConcurrentQueue<KeyValuePair<DateTime, DateTime>> keyValuePairs;
        //private ConcurrentBag<CloudflareLogReport> cloudflareLogReports;

        //private string[] agentUrlArr = new string[] { "liveChathanlder3.ashx", "errorcollector.ashx", "formbuilder.ashx", "formconsumer.ashx", "FileUploadHandler.ashx" };
        private double sample = 1;
        //private List<RateLimitEntity> rateLimits;
        private int globalThreshold = 100;
        private int globalPeriod = 60;
        private int timeSpan = 60;

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
            var globalConfiguration = GlobalConfigBusiness.Get();
            var timeSpan = 60; //unit is second

            //var zoneList = ZoneBusiness.GetAllList();

            var zoneEntity = new ZoneEntity() {
                ZoneId = "2068c8964a4dcef78ee5103471a8db03",
                AuthEmail = "elei.xu@comm100.com",
                AuthKey = "1e26ac28b9837821af730e70163f0604b4c35",
                IfTestStage = true

    };
            globalThreshold = globalConfiguration.GlobalThreshold;
            globalPeriod = globalConfiguration.GlobalPeriod;
            sample = globalConfiguration.GlobalSample;
            timeSpan = globalConfiguration.GlobalTimeSpan;

            #region Start Analyze Log
            StartAnalyze(zoneEntity);
            #endregion


            return Task.FromResult(0);
        }

        private void StartAnalyze(ZoneEntity zoneEntity)
        {
            var dtStart = DateTime.Now.AddDays(-1).AddHours(-10).AddMinutes(-56);
            var dtEnd = DateTime.Now.AddDays(-1).AddHours(-10).AddMinutes(-55).AddSeconds(-1);

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

                var timeStage = string.Format("{0}-{1}", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));
                Console.WriteLine(string.Format("开始获取[{0}]的日志", timeStage));
                AuditLogBusiness.InsertLog(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("开始获取[{0}]的日志", timeStage)));
                
                cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var retry);

                while (retry && retryCount < 10)
                {
                    retryCount++;
                    cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
                }
                Console.WriteLine(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("获取[{0}]的日志结束,总计[{1}]条", timeStage, cloudflareLogs.Count)).Detail);
                AuditLogBusiness.InsertLog(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("获取[{0}]的日志结束,总计[{1}]条", timeStage, cloudflareLogs.Count)));

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

                    Console.WriteLine(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("除去白名单后日志总计[{0}]条", requestDetailList.Count)).Detail);
                    AuditLogBusiness.InsertLog(new AuditLogEntity(zoneEntity.ZoneId, LogLevel.App, string.Format("除去白名单后日志总计[{0}]条", requestDetailList.Count)));

                    if (requestDetailList.Count > 0)
                    {
                        AnalyzeLog(requestDetailList, zoneEntity, rateLimits);
                    }
                    
                    #endregion
                }
            }
        }

        private void AnalyzeLog(List<LogAnalyzeModel> logsAll, ZoneEntity zoneEntity, List<RateLimitEntity> rateLimits)
        {
            var systemLogList = new List<AuditLogEntity>();
            var zoneId = zoneEntity.ZoneId;
            var ifTestStage = zoneEntity.IfTestStage;
            try
            {
                CloudflareAccessRuleResponse cloudflareAccessRuleResponse = null;
                var cloudflare = new CloudflareBusiness(zoneId, zoneEntity.AuthEmail, zoneEntity.AuthKey);
                var alertFlag = false;
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, "开始日志分析"));
                var dtNow = DateTime.Now;
                var logsIpAll = logsAll.GroupBy(x => new { x.IP, x.RequestHost }).Select(x => new LogAnalyzeModel()
                {
                    IP = x.Key.IP,
                    RequestHost = x.Key.RequestHost,
                    RequestCount = x.Count()
                }).Where(x => x.RequestCount / (float)(timeSpan * sample) >= ((float)globalThreshold / globalPeriod)).ToList();

                if (logsIpAll.Count() > 0)
                {
                    AlertCall(out alertFlag);

                    var sbLog = new StringBuilder(string.Format("访问超过全局阈值[Threshold={1},Period={2}]的IP共{0}个，分别为:", logsIpAll.Count(), globalThreshold, globalPeriod));
                    foreach (var rule in logsIpAll)
                    {
                        sbLog.AppendFormat("[{0}({1}次)]; ", rule.IP, rule.RequestCount);
                    }
                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbLog.ToString()));

                    foreach (var rule in logsIpAll)
                    {
                        var ipRequestList = logsAll.Where(x=>x.IP.Equals(rule.IP)).GroupBy(x => new { x.RequestFullUrl }).Select(x => new LogAnalyzeModel()
                        {
                            RequestFullUrl = x.Key.RequestFullUrl,
                            RequestCount = x.Count()
                        }).ToList().OrderByDescending(x=>x.RequestCount);

                        foreach (var ipRequestUrl in ipRequestList)
                        {
                            systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("IP [{0}] 请求了地址 [{1}] {2}次", rule.IP, ipRequestUrl.RequestFullUrl, ipRequestUrl.RequestCount)));
                        }
                    }


                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, "开始Ban IP操作"));
                    foreach (var rule in logsIpAll)
                    {
                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("开始Ban IP [{0}];", rule.IP)));

                        if (ifTestStage)
                        {
                            systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Ban IP [{0}] 成功, 测试阶段只打印日志;", rule.IP)));
                        }
                        else
                        {
                            cloudflareAccessRuleResponse = cloudflare.BanIp(rule.IP, "Ban Ip By Attack Prevent Windows service!");
                            if (cloudflareAccessRuleResponse.Success)
                            {
                                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Ban IP [{0}] 成功;", rule.IP)));
                            }
                            else
                            {
                                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Ban IP [{0}] 失败, 原因是：[{1}]", rule.IP, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "Cloudflare没有返回错误信息")));
                            }
                        }
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
                
                foreach (var rateLimit in rateLimits)
                {
                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("开始分析规则ID={0},Url={1}", rateLimit.ID, rateLimit.Url)));
                    //抽取出所有ratelimit规则中的请求列表
                    var logAnalyzeDetailList = logs.Where(x => x.RequestUrl.Equals(rateLimit.Url)).ToList();

                    //对IP的请求地址(不包含querystring)进行分组
                    var ipRequestListIncludingQueryString = logAnalyzeDetailList.GroupBy(x => new { x.IP, x.RequestUrl, x.RequestFullUrl }).Select(x => new LogAnalyzeModel()
                    {
                        IP = x.Key.IP,
                        RequestUrl = x.Key.RequestUrl,
                        RequestFullUrl = x.Key.RequestFullUrl,
                        RequestCount = x.Count()
                    }).ToList();

                    //对IP的请求地址(不包含querystring)进行分组
                    var ipRequestList = logAnalyzeDetailList.GroupBy(x => new { x.IP, x.RequestUrl }).Select(x => new LogAnalyzeModel()
                    {
                        IP = x.Key.IP,
                        RequestUrl = x.Key.RequestUrl,
                        RequestCount = x.Count()
                    }).ToList();

                    //抽取出所有违反规则的IP请求列表
                    var brokenRuleIpList = (from item in ipRequestList
                                            where item.RequestUrl.ToLower().Equals(rateLimit.Url.ToLower())
                                                  && item.RequestCount / (float)(timeSpan * sample) >= (rateLimit.Threshold * rateLimit.EnlargementFactor / (float)rateLimit.Period)
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
                        AlertCall(out alertFlag);

                        var sbLog = new StringBuilder(string.Format("超过RateLimit阈值(Threshold={1},Period={2})的Ip请求有 {0} 条,分别为：", brokenRuleIpList.Count, rateLimit.Threshold, rateLimit.Period));
                        foreach (var rule in logsIpAll)
                        {
                            sbLog.AppendFormat("[{0}({1}次)]; ", rule.IP, rule.RequestCount);
                        }
                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, sbLog.ToString()));

                        foreach (var rule in brokenRuleIpList)
                        {
                            var ipRequestUrlList = ipRequestListIncludingQueryString.Where(x => x.IP.Equals(rule.IP)).ToList();
                            foreach (var ipRequestUrl in ipRequestUrlList)
                            {
                                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("IP [{0}] 请求了地址 [{1}] {2}次", rule.IP, ipRequestUrl.RequestFullUrl, ipRequestUrl.RequestCount)));
                            }
                        }

                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("开始打开Cloudflare的RateLimit规则[URL=[{0}],Threshold=[{1}],Period=[{2}]]", rateLimit.Url, rateLimit.Threshold, rateLimit.Period)));
                        if (ifTestStage)
                        {
                            systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("打开Cloudflare的RateLimit规则[URL=[{0}],Threshold=[{1}],Period=[{2}]]成功, 测试阶段只打印日志;", rateLimit.Url, rateLimit.Threshold, rateLimit.Period)));
                        }
                        else
                        {

                            if (cloudflare.OpenRateLimit(rateLimit.Url, rateLimit.Threshold, rateLimit.Period, out var openRateLimitLogs))
                            {
                                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("打开Cloudflare的RateLimit规则[URL=[{0}],Threshold=[{1}],Period=[{2}]]成功;", rateLimit.Url, rateLimit.Threshold, rateLimit.Period)));
                            }
                            else
                            {
                                systemLogList.AddRange(openRateLimitLogs);
                            }
                        }

                        systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("开始BanIp操作")));
                        foreach (var rule in brokenRuleIpList)
                        {
                            systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("开始Ban IP [{0}];", rule.IP)));

                            if (ifTestStage)
                            {
                                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Ban IP [{0}] 成功, 测试阶段只打印日志;", rule.IP)));
                            }
                            else
                            {
                                cloudflareAccessRuleResponse = cloudflare.BanIp(rule.IP, "Ban Ip By Attack Prevent Windows service!");
                                if (cloudflareAccessRuleResponse.Success)
                                {
                                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Ban IP [{0}] 成功;", rule.IP)));
                                }
                                else
                                {
                                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Audit, string.Format("Ban IP [{0}] 失败, 原因是：[{1}]", rule.IP, cloudflareAccessRuleResponse.Errors.Count() > 0 ? cloudflareAccessRuleResponse.Errors[0] : "Cloudflare没有返回错误信息")));
                                }
                            }
                        }
                    }

                    systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.App, string.Format("分析规则ID={0},Url={1}结束", rateLimit.ID, rateLimit.Url)));
                }

            }
            catch (Exception ex)
            {
                systemLogList.Add(new AuditLogEntity(zoneId, LogLevel.Error, string.Format("程序出现错误，原因是:[{0}]", ex.Message)));
            }
            finally
            {
                AuditLogBusiness.InsertLogs(systemLogList);
                foreach (var log in systemLogList)
                {
                    Console.WriteLine(log.Detail);
                }
            }
        }

        private void AlertCall(out bool alertFlag)
        {
            alertFlag = true;
            Console.WriteLine("异步修改预警标志，开始报警！");
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

        //private bool IfInRateLimitRule(string requestUrl)
        //{
        //    if (null != rateLimits)
        //    {
        //        var result = rateLimits.Where(x => requestUrl.Contains(x.Url));
        //        if (result != null && result.Count() > 0) return true;
                   
        //    }
        //    return false;
        //}

        //public void PushReport(CloudflareLogReport CloudflareLogReport)
        //{
        //    if (cloudflareLogReports == null)
        //    {
        //        cloudflareLogReports = new ConcurrentBag<CloudflareLogReport>();
        //    }

        //    cloudflareLogReports.Add(CloudflareLogReport);

        //}

        ////获取实时报表
        //public CloudflareLogReport GetCloudflareLogReport(DateTime startTime, DateTime endTime)
        //{
        //    CloudflareLogReport cloudflareLogReport = new CloudflareLogReport();
        //    try
        //    {
        //        Stopwatch stopwatch = new Stopwatch();

        //        var orderby = cloudflareLogReports.Where(a => a.Start >= startTime && a.End <= endTime).OrderBy(a => a.Time).ToList();
        //        CloudflareLogReport minCloudflareLogReport = orderby?.First();
        //        CloudflareLogReport maxCloudflareLogReport = orderby?.Last();

        //        DateTime start = minCloudflareLogReport.Start;
        //        DateTime end = maxCloudflareLogReport.End;
        //        int size = orderby.Sum(a => a.Size);
        //        string time = string.Format("{0}-{1}", start.ToString("yyyyMMddHHmmss"), end.ToString("yyyyMMddHHmmss"));

        //        var cloudflareLogReportItemsMany = orderby.SelectMany(a => a.CloudflareLogReportItems).ToList();

        //        //合并处理
        //        var itemsManyGroup = cloudflareLogReportItemsMany.GroupBy(a => new { a.ClientIP, a.ClientRequestHost, a.ClientRequestURI })
        //            .Select(g => new { g.Key.ClientRequestHost, g.Key.ClientIP, g.Key.ClientRequestURI, Ban = false, Count = g.Sum(c => c.Count) }).OrderByDescending(a => a.Count).ToList();

        //        List<IpNeedToBan> ipNeedToBans = new List<IpNeedToBan>();


        //        var banItems = new List<CloudflareLogReportItem>();

        //        var result = (from item in itemsManyGroup
        //                      from rateLimit in rateLimits
        //                      where item.ClientRequestURI.ToLower().Contains(rateLimit.Url.ToLower())
        //                            && ((item.Count / (float)((end - start).TotalSeconds * sample)) >= (rateLimit.Threshold * rateLimit.EnlargementFactor / (float)rateLimit.Period))
        //                      select new { item, rateLimit.ID }).OrderByDescending(a => a.item.Count).ToList();

        //        List<int> handleIdList = new List<int>();

        //        foreach (var item in result)
        //        {
        //            //存储rotelimit的触发log
        //            var roteLimit = rateLimits.FirstOrDefault(a => a.ID == item.ID);
        //            var containRoteLimitList = result.Where(a => a.ID == item.ID).ToList();
        //            //同时有N个IP达到触发某条规则时候开启本条规则或者创建
        //            var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
        //            //-todo 记录日志，自动打开RatelimitRule
        //            //cloudflare.OpenRateLimit(roteLimit.Url, roteLimit.Threshold, roteLimit.Period);
        //            //-todo 记录日志，自动Ban Ip
        //            //cloudflare.BanIp(item.item.ClientIP,"Ban notes");

        //        }

        //        cloudflareLogReport = new CloudflareLogReport
        //        {
        //            Guid = Guid.NewGuid().ToString(),
        //            Time = time,
        //            Start = start,
        //            End = end,
        //            Size = size,
        //            CloudflareLogReportItems = banItems.ToArray()
        //        };

        //        stopwatch.Stop();
        //        //OnMessage(new MessageEventArgs("报表汇总用时:" + stopwatch.ElapsedMilliseconds / 1000 + "秒"));

        //    }
        //    catch (Exception e)
        //    {
        //        //logger.Error(e.Message);
        //    }

        //    return cloudflareLogReport;
        //}

        //private void InitQueue(DateTime dtStart, DateTime dtEnd, int timeSpan)
        //{
        //    if (keyValuePairs == null)
        //    {
        //        keyValuePairs = new ConcurrentQueue<KeyValuePair<DateTime, DateTime>>();
        //    }
        //    DateTime dateTime = dtStart;
        //    //OnMessage(new MessageEventArgs("产生队列数据开始"));
        //    while (true)
        //    {
        //        string time = string.Format("{0}-{1}", dateTime.ToString("yyyyMMddHHmmss"), dateTime.AddSeconds(timeSpan).ToString("yyyyMMddHHmmss"));

        //        keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dateTime, dateTime.AddSeconds(timeSpan)));

        //        dateTime = dateTime.AddSeconds(timeSpan);

        //        if (dateTime >= dtEnd)
        //        {
        //            break;
        //        }
        //    }
        //    //OnMessage(new MessageEventArgs("产生队列数据结束"));
        //}

        //private void TaskStart(int taskCount, string zoneId, string authEmail, string authKey, double sample, string[] agentUrlArr)
        //{
        //    List<Task> taskList = new List<Task>(taskCount);
        //    for (var i = 0; i < taskCount; i++)
        //    {
        //        var task = Task.Factory.StartNew(() =>
        //        {
        //            Dequeue(zoneId, authEmail, authKey, sample, agentUrlArr);
        //        });
        //        taskList.Add(task);

        //        Thread.Sleep(500);
        //    }

        //    Task.WaitAll(taskList.ToArray());//等待所有线程只都行完毕

        //}

        //private void Dequeue(string zoneId, string authEmail, string authKey, double sample, string[] agentUrlArr)
        //{
        //    try
        //    {
        //        while (true)
        //        {

        //            //OnMessage(new MessageEventArgs("取队列数据处理开始"));
        //            var stopwatch = new Stopwatch();
        //            stopwatch.Start();
        //            var retryCount = 0;
        //            if (keyValuePairs.TryDequeue(out KeyValuePair<DateTime, DateTime> keyValuePair))
        //            {
        //                DateTime dtStart = keyValuePair.Key;
        //                DateTime dtEnd = keyValuePair.Value;

        //                var timeStage = string.Format("{0}-{1}", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));
        //                Console.WriteLine(string.Format("开始获取[{0}]的日志", timeStage));
        //                AuditLogBusiness.InsertLog(new AuditLogEntity(zoneId, LogLevel.App, string.Format("开始获取[{0}]的日志",timeStage)));
        //                var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
        //                var cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var retry);

        //                while (retry && retryCount < 10)
        //                {
        //                    retryCount++;
        //                    cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
        //                }
        //                Console.WriteLine(string.Format("获取[{0}]的日志结束", timeStage));
        //                AuditLogBusiness.InsertLog(new AuditLogEntity(zoneId, LogLevel.App, string.Format("获取[{0}]的日志结束", timeStage)));
        //                #region 分析日志
        //                var requestDetailList = cloudflareLogs.Select(x => new LogAnalyzeModel()
        //                {
        //                    IP = x.ClientIP,
        //                    RequestHost = x.ClientRequestHost,
        //                    RequestFullUrl = string.Format("{0}{1}", x.ClientRequestHost, x.ClientRequestURI),
        //                    RequestUrl = string.Format("{0}{1}", x.ClientRequestHost, x.ClientRequestURI.IndexOf('?') > 0 ? x.ClientRequestURI.Substring(0, x.ClientRequestURI.IndexOf('?')) : x.ClientRequestURI)
        //                }).ToList();

        //                AnalyzeLog(requestDetailList);
        //                #endregion
        //            }
        //            else
        //            {
        //                break;
        //            }

        //            //OnMessage(new MessageEventArgs("取队列数据处理结束"));

        //        }
        //    }
        //    catch (Exception )
        //    {
        //        //logger.Error(e.Message);
        //    }

        //}
    }
}
