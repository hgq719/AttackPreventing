using AttackPrevent.Business;
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
        public Task Execute(IJobExecutionContext context)
        {
            var zoneId = "2068c8964a4dcef78ee5103471a8db03";
            var authEmail = "elei.xu@comm100.com";
            var authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            var agentUrlArr = new string[] { "liveChathanlder3.ashx", "errorcollector.ashx", "formbuilder.ashx", "formconsumer.ashx", "FileUploadHandler.ashx" };
            double sample = 1;
            var timeSpan = 60; //unit is second

            #region Get White List
            var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
            var whitelist = cloudflare.GetAccessRules();
            var ipList = cloudflare.GetWhitelist(whitelist);
            #endregion

            #region Get logs
            InitQueue(timeSpan);

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

        private void InitQueue(int timeSpan)
        {
            if (keyValuePairs == null)
            {
                keyValuePairs = new ConcurrentQueue<KeyValuePair<DateTime, DateTime>>();
            }
            var dtYesterday = DateTime.Now.AddDays(-1);
            var dtStart = new DateTime(dtYesterday.Year, dtYesterday.Month, dtYesterday.Day, 0, 0, 0);
            var dtEnd = dtStart.AddSeconds(timeSpan);
            for (int i = 0; i < 1; i++)
            {
                keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dtStart.AddHours(i), dtEnd.AddHours(i)));
            }
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

                        //string time = string.Format("{0}-{1}", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));
                        var cloudflare = new CloudflareBusiness(zoneId, authEmail, authKey);
                        var cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out var retry);

                        while (retry && retryCount < 10)
                        {
                            retryCount++;
                            cloudflareLogs = cloudflare.GetLogs(dtStart, dtEnd, 1, out retry);
                        }

                        if (null != cloudflareLogs && cloudflareLogs.Count > 0)
                        {
                            var list = cloudflareLogs.Where(x => x.ClientRequestURI.Contains("livechathandler3.ashx")
                            || x.ClientRequestURI.Contains("errorcollector.ashx")
                            || x.ClientRequestURI.Contains("formbuilder.ashx")
                            || x.ClientRequestURI.Contains("formconsumer.ashx")
                            || x.ClientRequestURI.Contains("FileUploadHandler.ashx")
                            ).Select(x => new { x.ClientIP, x.ClientRequestURI, x.ClientRequestHost });

                        }

                        stopwatch.Stop();

                    }
                    else
                    {
                        break;
                    }

                    //OnMessage(new MessageEventArgs("取队列数据处理结束"));

                }
            }
            catch (Exception e)
            {
                //logger.Error(e.Message);
            }

        }
    }
}
