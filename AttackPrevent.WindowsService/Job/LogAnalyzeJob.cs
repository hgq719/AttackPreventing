using AttackPrevent.Business;
using AttackPrevent.Model.Cloudflare;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService.Job
{
    public class LogAnalyzeJob : IJob
    {
        private ConcurrentQueue<KeyValuePair<DateTime, DateTime>> keyValuePairs;
        private ConcurrentBag<CloudflareLogReport> cloudflareLogReports;
        private string zoneId = "2068c8964a4dcef78ee5103471a8db03";
        private string authEmail = "elei.xu@comm100.com";
        private string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
        private string[] agentUrlArr = new string[] { "liveChathanlder3.ashx", "errorcollector.ashx", "formbuilder.ashx", "formconsumer.ashx", "FileUploadHandler.ashx" };
        double sample = 1;

        public Task Execute(IJobExecutionContext context)
        {
           
            var timeSpan = 60; //unit is second
            var dtStart = DateTime.Now.AddMinutes(-9);
            var dtEnd = DateTime.Now.AddMinutes(-8);

            #region Get White List
            var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
            var ipList = cloudflare.GetIpWhitelist();
            #endregion

            #region Get logs
            InitQueue(dtStart, dtEnd, timeSpan);

            TaskStart(4, zoneId, authEmail, authKey, sample, agentUrlArr);
            #endregion

            #region Analyze logs and extract white list

            #endregion

            #region Send email

            #endregion

            #region Generate email content

            #endregion

            throw new NotImplementedException();
        }

        private void InitQueue(DateTime dtStart, DateTime dtEnd, int timeSpan)
        {
            if (keyValuePairs == null)
            {
                keyValuePairs = new ConcurrentQueue<KeyValuePair<DateTime, DateTime>>();
            }
            DateTime dateTime = dtStart;
            //OnMessage(new MessageEventArgs("产生队列数据开始"));
            while (true)
            {
                string time = string.Format("{0}-{1}", dateTime.ToString("yyyyMMddHHmmss"), dateTime.AddSeconds(timeSpan).ToString("yyyyMMddHHmmss"));

                if (cloudflareLogReports != null)
                {
                    if (!cloudflareLogReports.Any(a => a.Time == time))
                    {
                        //OnMessage(new MessageEventArgs(time));
                        keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dateTime, dateTime.AddSeconds(timeSpan)));
                    }
                }
                else
                {
                    //OnMessage(new MessageEventArgs(time));
                    keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dateTime, dateTime.AddSeconds(timeSpan)));
                }

                dateTime = dateTime.AddSeconds(timeSpan);

                if (dateTime >= dtEnd)
                {
                    break;
                }
            }
            //OnMessage(new MessageEventArgs("产生队列数据结束"));
        }

        private void TaskStart(int taskCount, string zoneId, string authEmail, string authKey, double sample, string[] agentUrlArr)
        {
            List<Task> taskList = new List<Task>(taskCount);
            for (var i = 0; i < taskCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    Dequeue(zoneId, authEmail, authKey, sample, agentUrlArr);
                });
                taskList.Add(task);

                Thread.Sleep(500);
            }

            Task.WaitAll(taskList.ToArray());//等待所有线程只都行完毕

        }

        public void Dequeue(string zoneId, string authEmail, string authKey, double sample, string[] agentUrlArr)
        {
            try
            {
                while (true)
                {

                    //OnMessage(new MessageEventArgs("取队列数据处理开始"));
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var retryCount = 0;
                    if (keyValuePairs.TryDequeue(out KeyValuePair<DateTime, DateTime> keyValuePair))
                    {
                        DateTime dtStart = keyValuePair.Key;
                        DateTime dtEnd = keyValuePair.Value;

                        string time = string.Format("{0}-{1}", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));
                        var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
                        var cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var retry);

                        while (retry && retryCount < 10)
                        {
                            retryCount++;
                            cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
                        }

                        #region 分析日志
                        //OnMessage(new MessageEventArgs("取出数据:" + time + "共" + cloudflareLogs.Count + "条,用时:" + stopwatch.ElapsedMilliseconds / 1000 + "秒"));

                        stopwatch.Restart();

                        var result = cloudflareLogs.GroupBy(a => new { a.ClientRequestHost, a.ClientIP, a.ClientRequestURI }).
                            Select(g => new CloudflareLogReportItem
                            {
                                ClientRequestHost = g.Key.ClientRequestHost,
                                ClientIP = g.Key.ClientIP,
                                ClientRequestURI = g.Key.ClientRequestURI,
                                Count = g.Count()
                            });

                        var CloudflareLogReport = new CloudflareLogReport
                        {
                            Guid = Guid.NewGuid().ToString(),
                            Time = time,
                            Start = dtStart,
                            End = dtEnd,
                            Size = cloudflareLogs.Count,
                            CloudflareLogReportItems = result.ToArray()
                        };
                        stopwatch.Stop();
                        //OnMessage(new MessageEventArgs("分析" + time + "用时:" + stopwatch.ElapsedMilliseconds / 1000 + "秒"));

                        PushReport(CloudflareLogReport);
                        #endregion

                        stopwatch.Stop();

                    }
                    else
                    {
                        break;
                    }

                    //OnMessage(new MessageEventArgs("取队列数据处理结束"));

                }
            }
            catch (Exception )
            {
                //logger.Error(e.Message);
            }

        }

        public void PushReport(CloudflareLogReport CloudflareLogReport)
        {
            if (cloudflareLogReports == null)
            {
                cloudflareLogReports = new ConcurrentBag<CloudflareLogReport>();
            }

            cloudflareLogReports.Add(CloudflareLogReport);

        }

        //获取实时报表
        public CloudflareLogReport GetCloudflareLogReport(DateTime startTime, DateTime endTime)
        {
            CloudflareLogReport cloudflareLogReport = new CloudflareLogReport();
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                var orderby = cloudflareLogReports.Where(a => a.Start >= startTime && a.End <= endTime).OrderBy(a => a.Time).ToList();
                CloudflareLogReport minCloudflareLogReport = orderby?.First();
                CloudflareLogReport maxCloudflareLogReport = orderby?.Last();

                DateTime start = minCloudflareLogReport.Start;
                DateTime end = maxCloudflareLogReport.End;
                int size = orderby.Sum(a => a.Size);
                string time = string.Format("{0}-{1}", start.ToString("yyyyMMddHHmmss"), end.ToString("yyyyMMddHHmmss"));

                var cloudflareLogReportItemsMany = orderby.SelectMany(a => a.CloudflareLogReportItems).ToList();

                //合并处理
                var itemsManyGroup = cloudflareLogReportItemsMany.GroupBy(a => new { a.ClientIP, a.ClientRequestHost, a.ClientRequestURI })
                    .Select(g => new { g.Key.ClientRequestHost, g.Key.ClientIP, g.Key.ClientRequestURI, Ban = false, Count = g.Sum(c => c.Count) }).OrderByDescending(a => a.Count).ToList();

                List<IpNeedToBan> ipNeedToBans = new List<IpNeedToBan>();


                //var banItems = new List<CloudflareLogReportItem>();
                               
                //var result = (from item in itemsManyGroup
                //              from config in roteLimitConfig.RateLimits
                //              where item.ClientRequestURI.ToLower().Contains(config.Url.ToLower())
                //                    && ((item.Count / (float)((end - start).TotalSeconds * sample)) >= (config.LimitTimes * roteLimitConfig.TriggerRatio / (float)config.Interval))
                //              select new { item, config.Id }).OrderByDescending(a => a.item.Count).ToList();

                //List<int> handleIdList = new List<int>();

                //foreach (var item in result)
                //{
                //    var ban = false;

                //    //存储rotelimit的触发log
                //    var roteLimit = roteLimitConfig.RateLimits.FirstOrDefault(a => a.Id == item.Id);
                //    var ipNumber = 0;
                //    var action = "None";//None/Create/Delete
                //    var containRoteLimitList = result.Where(a => a.Id == item.Id).ToList();
                //    //ipNumber = containRoteLimitList.Count;
                //    //同时有N个IP达到触发某条规则时候开启本条规则或者创建
                //    var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
                //    cloudflare.OpenRateLimit(roteLimit.Url, roteLimit.Threshold, roteLimit.Period);

                //    cloudflare.BanIp(item.item.ClientIP);

                //}

                //cloudflareLogReport = new CloudflareLogReport
                //{
                //    Guid = Guid.NewGuid().ToString(),
                //    Time = time,
                //    Start = start,
                //    End = end,
                //    Size = size,
                //    CloudflareLogReportItems = banItems.ToArray()
                //};

                stopwatch.Stop();
                //OnMessage(new MessageEventArgs("报表汇总用时:" + stopwatch.ElapsedMilliseconds / 1000 + "秒"));

            }
            catch (Exception e)
            {
                //logger.Error(e.Message);
            }

            return cloudflareLogReport;
        }
    }
}
