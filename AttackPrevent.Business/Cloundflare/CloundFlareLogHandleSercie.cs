using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface ICloudflareLogHandleSercie
    {
        //void InitQueue(DateTime startTime, DateTime endTime);
        //开启任务
        void TaskStart();
        List<CloudflareLog> GetCloudflareLogs(string key);

        void CleanDicCloudflareLogsToday();
    }
    public class CloudflareLogHandleSercie : ICloudflareLogHandleSercie
    {
        private static ConcurrentDictionary<string, List<CloudflareLog>> dicCloudflareLogsToday = null;

        private ConcurrentDictionary<string, List<CloudflareLog>> dicCloudflareLogs = new ConcurrentDictionary<string, List<CloudflareLog>>();
        private ConcurrentQueue<KeyValuePair<DateTime, DateTime>> keyValuePairs;
        private ILogService logService;
        private double sample = 0.01;
        private int timeSpan = 20;
        private DateTime startTime;
        private DateTime endTime;
        private int taskCount = 3;
        private string zoneId;
        private string authEmail;
        private string authKey;

        private bool isStoreToday;//2020.7.10 

        public CloudflareLogHandleSercie(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end, bool isStoreToday=false)
        {
            this.zoneId = zoneId;
            this.authEmail = authEmail;
            this.authKey = authKey;
            this.sample = sample;
            this.startTime = start;
            this.endTime = end;

            this.isStoreToday = isStoreToday;

            _cloudFlareApiService = new CloudFlareApiService();
            logService = new Business.LogService();

            if (dicCloudflareLogsToday == null)
            {
                dicCloudflareLogsToday = new ConcurrentDictionary<string, List<CloudflareLog>>();
            }
        }

        private ICloudFlareApiService _cloudFlareApiService;

        //产生队列数据
        public void InitQueue(DateTime startTime, DateTime endTime)
        {
            if (keyValuePairs == null)
            {
                keyValuePairs = new ConcurrentQueue<KeyValuePair<DateTime, DateTime>>();
            }
            DateTime dateTime = startTime;

            while (true)
            {
                string time = string.Format("{0}-{1}", dateTime.ToString("yyyyMMddHHmmss"), dateTime.AddSeconds(timeSpan).ToString("yyyyMMddHHmmss"));

                keyValuePairs.Enqueue(new KeyValuePair<DateTime, DateTime>(dateTime, dateTime.AddSeconds(timeSpan)));

                dateTime = dateTime.AddSeconds(timeSpan);

                if (dateTime >= endTime)
                {
                    break;
                }
            }
        }
        //取队列数据处理
        public void Dequeue()
        {
            while (true)
            {
                KeyValuePair<DateTime, DateTime> keyValuePair = default(KeyValuePair<DateTime, DateTime>);
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (keyValuePairs.TryDequeue(out keyValuePair))
                    {
                        DateTime start = keyValuePair.Key;
                        DateTime end = keyValuePair.Value;

                        string time = string.Format("{0}-{1}", start.ToString("yyyyMMddHHmmss"), end.ToString("yyyyMMddHHmmss"));
                        bool retry = false;

                    string key = string.Format("{0}-{1}-{2}-{3}", startTime.ToString("yyyyMMddHHmmss"), endTime.ToString("yyyyMMddHHmmss"), sample, zoneId);
                    
                    if (isStoreToday
                        && dicCloudflareLogsToday.ContainsKey(time))
                    {//取缓存
                        if (!dicCloudflareLogs.Keys.Contains(key))
                        {
                            dicCloudflareLogs.TryAdd(key, new List<CloudflareLog>());
                        }
                            dicCloudflareLogs[key].AddRange(dicCloudflareLogsToday[time]);
                            continue;
                    }

                        List<CloudflareLog> cloudflareLogs = _cloudFlareApiService.GetCloudflareLogs(zoneId, authEmail, authKey, sample, start, end, out retry);
                        while (retry == true)
                        {
                            cloudflareLogs = _cloudFlareApiService.GetCloudflareLogs(zoneId, authEmail, authKey, sample, start, end, out retry);
                            Thread.Sleep(1000 * 5); //如果是频率限制，休眠5S，再请求
                        }

                        
                        if (!dicCloudflareLogs.Keys.Contains(key))
                        {
                            dicCloudflareLogs.TryAdd(key, new List<CloudflareLog>());
                        }

                        if (cloudflareLogs != null && cloudflareLogs.Count > 0)
                        {
                            dicCloudflareLogs[key].AddRange(cloudflareLogs);
                        }

                    if (isStoreToday
                        && cloudflareLogs.Count > 0)
                    {//记录缓存
                        if (!dicCloudflareLogsToday.Keys.Contains(time))
                        {
                            dicCloudflareLogsToday.TryAdd(time, new List<CloudflareLog>());
                            dicCloudflareLogsToday[time].AddRange(cloudflareLogs);
                        }           
                    }

                        stopwatch.Stop();

                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logService.Error(e.Message);// 2020.7.13 打印错误日志
                    throw e;
                    //keyValuePairs.Enqueue(keyValuePair); //异常重新入队列
                }
            }

        }
        //开启任务
        public void TaskStart()
        {
            InitQueue(startTime, endTime);

            List<Task> taskList = new List<Task>(taskCount);
            for (var i = 0; i < taskCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    Dequeue();
                });
                taskList.Add(task);

                Thread.Sleep(500);
            }

            Task.WaitAll(taskList.ToArray());//等待所有线程只都行完毕
        }

        public List<CloudflareLog> GetCloudflareLogs(string key)
        {
            return dicCloudflareLogs[key];
        }
        public void CleanDicCloudflareLogsToday()
        {//清空缓存 防止长时间占用内存
            dicCloudflareLogsToday.Clear();
        }
    }
}
