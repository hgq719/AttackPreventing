﻿using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface IEtwAnalyzeService
    {
        Task Add(string ip, ConcurrentBag<byte[]> data);
        void DoWork();
    }

    public class EtwAnalyzeService : IEtwAnalyzeService
    {
        private static IEtwAnalyzeService _etwAnalyzeService;
        private static object obj_Sync = new object();
        private ConcurrentQueue<EtwData> datas;
        private List<EtwData> handledDatas;
        private ILogService logger = new LogService();
        private readonly string _authKey = "EEF1BFC8-177C-424E-8F05-AFC08DEFBAC3";
        private readonly int ReceivingThreshold = 300;
        private string getRatelimitsApiUrl;
        private string analyzeResultApiUrl;
        private string awsGetWhiteListApiUrl;
        private string awsGetZoneListApiUrl;
        private int accumulationSecond;
        private volatile bool ifBusy = false;
        private bool parseEtwDataLog = false;
        private int receiveCount = 0;
        private static object receiveSync = new object();

        private EtwAnalyzeService()
        {
            getRatelimitsApiUrl = ConfigurationManager.AppSettings["AwsGetRatelimitsApiUrl"] ?? "http://localhost:41967/GetZones/{zoneId}/Ratelimits";
            analyzeResultApiUrl = ConfigurationManager.AppSettings["AwsAnalyzeResultApiUrl"] ?? "http://localhost:41967/IISLogs/AnalyzeResult";
            awsGetWhiteListApiUrl = ConfigurationManager.AppSettings["AwsGetWhiteListApiUrl"] ?? "http://localhost:41967/GetWhiteList/{zoneId}/WhiteLists";
            accumulationSecond = int.Parse(ConfigurationManager.AppSettings["AccumulationSecond"] ?? "20");//累计秒
            awsGetZoneListApiUrl = ConfigurationManager.AppSettings["AwsGetZoneListApiUrl"] ?? "http://localhost:41967/GetZoneList/Zones";

            datas = new ConcurrentQueue<EtwData>();
            handledDatas = new List<EtwData>();
        }

        public static IEtwAnalyzeService GetInstance()
        {
            if (_etwAnalyzeService != null) return _etwAnalyzeService;
            lock (obj_Sync)
            {
                if (_etwAnalyzeService == null)
                {
                    _etwAnalyzeService = new EtwAnalyzeService();
                }
            }
            return _etwAnalyzeService;
        }

        public Task Add(string ip, ConcurrentBag<byte[]> data)
        {
            return Task.Run(() =>
            {
                //设置一个接收阀值
                var queueCount = datas.Count();
                if (queueCount < ReceivingThreshold)
                {
                    var etwData = new EtwData
                    {
                        guid = Guid.NewGuid().ToString(),
                        buffList = data,
                        enumEtwStatus = EnumEtwStatus.None,
                        time = DateTime.Now.Ticks,
                        retryCount = 0,
                        senderIp = ip,
                    };
                    datas.Enqueue(etwData);
                }
                else
                {
                    //logger.Debug(JsonConvert.SerializeObject(
                    //    $"EtwData Add more than max=[{ReceivingThreshold}]"
                    //));
                }

                lock (receiveSync)
                {
                    receiveCount++;
                    logger.Info(JsonConvert.SerializeObject(new {
                        receiveCount
                    }));
                }
            });
        }

        public void DoWork()
        {
            try
            {
                if (!ifBusy) //code review by michael, 这里会有多线程的问题吗? 如果有的话，要加locker.
                {
                    ifBusy = true;
                    DoWorkOne();
                    DoWorkAccumulation();
                    ifBusy = false;
                }
            }
            catch(Exception e)
            {
                logger.Error(e.StackTrace); //Code review by michael. 只打印堆栈，没有打印message.
                ifBusy = false;
            }
            finally
            {

            }

            //List<Task> taskList = new List<Task>(2);
            //var task = Task.Factory.StartNew(() =>
            //{
            //    doWorkOne();
            //});
            //taskList.Add(task);
            //Thread.Sleep(500);

            //task = Task.Factory.StartNew(() =>
            //{
            //    doWorkAccumulation();
            //});
            //taskList.Add(task);

            //Task.WaitAll(taskList.ToArray());//等待所有线程只都行完毕

            ////开启两个线程
            //Task.Run(() =>
            //{
            //    EtwData data = null;
            //    while (true)
            //    {
            //        try
            //        {
            //            Stopwatch stopwatch = new Stopwatch();
            //            stopwatch.Start();
            //            data = datas.Where(a => a.enumEtwStatus == EnumEtwStatus.None)
            //                .OrderByDescending(a => a.time).FirstOrDefault();
            //            if (data != null)
            //            {
            //                data.enumEtwStatus = EnumEtwStatus.Processing;
            //                Analyze(data);
            //                data.enumEtwStatus = EnumEtwStatus.Processed;
            //                stopwatch.Stop();
            //                logger.Debug(JsonConvert.SerializeObject(new
            //                {
            //                    index = data.time,
            //                    time = stopwatch.Elapsed.TotalMilliseconds
            //                }));
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            logger.Error(e.StackTrace);
            //            if (data != null)
            //            {
            //                data.retryCount += 1;
            //                if (data.retryCount > 5)
            //                {
            //                    data.enumEtwStatus = EnumEtwStatus.Failed;
            //                    logger.Error(JsonConvert.SerializeObject(data));
            //                }
            //                else
            //                {
            //                    data.enumEtwStatus = EnumEtwStatus.None;
            //                }
            //            }
            //        }
            //    }
            //});
            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        try
            //        {
            //            //Stopwatch stopwatch = new Stopwatch();
            //            //stopwatch.Start();
            //            //var list = datas.Where(a => a.enumEtwStatus == EnumEtwStatus.Processed).Take(accumulationSecond).ToList();
            //            //if (list != null && list.Count == accumulationSecond)
            //            //{
            //            //    AnalyzeAccumulation(list);
            //            //    stopwatch.Stop();
            //            //    logger.Debug(JsonConvert.SerializeObject(new
            //            //    {
            //            //        time = stopwatch.Elapsed.TotalMilliseconds
            //            //    }));
            //            //    foreach (EtwData etwData in list)
            //            //    {
            //            //        var etw = etwData;
            //            //        datas.TryTake(out etw);
            //            //    }
            //            //}


            //            var list = datas.Where(a => a.enumEtwStatus == EnumEtwStatus.Processed)
            //                            .GroupBy(a => a.senderIp)
            //                            .Where(g => g.Count() >= accumulationSecond);

            //            if (list != null && list.Count() > 0)
            //            {
            //                foreach (var gp in list)
            //                {
            //                    Stopwatch stopwatch = new Stopwatch();
            //                    stopwatch.Start();
            //                    var dataList = gp.Take(accumulationSecond).ToList();
            //                    AnalyzeAccumulation(dataList);
            //                    stopwatch.Stop();
            //                    logger.Debug(JsonConvert.SerializeObject(new
            //                    {
            //                        time = stopwatch.Elapsed.TotalMilliseconds
            //                    }));
            //                    foreach (EtwData etwData in list)
            //                    {
            //                        var etw = etwData;
            //                        datas.TryTake(out etw);
            //                    }
            //                }
            //            }

            //        }
            //        catch (Exception e)
            //        {
            //            logger.Error(e.StackTrace);
            //        }
            //    }
            //});
        }

        private void DoWorkOne()
        {

            EtwData data = null;
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                //data = datas.Where(a => a.enumEtwStatus == EnumEtwStatus.None)
                //    .OrderByDescending(a => a.time).FirstOrDefault();
                datas.TryDequeue(out data);
                if (data != null)
                {
                    data.enumEtwStatus = EnumEtwStatus.Processing;
                    Analyze(data);
                    data.enumEtwStatus = EnumEtwStatus.Processed;
                    stopwatch.Stop();
                    //logger.Info(JsonConvert.SerializeObject(new
                    //{
                    //    index = data.time,
                    //    time = stopwatch.Elapsed.TotalMilliseconds
                    //}));
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace); //Code review by michael, 为什么只打印堆栈不打印Message
                if (data != null)
                {
                    data.retryCount += 1;
                    if (data.retryCount > 5)
                    {
                        data.enumEtwStatus = EnumEtwStatus.Failed;
                        logger.Error(JsonConvert.SerializeObject(data));
                    }
                    else
                    {
                        data.enumEtwStatus = EnumEtwStatus.None;
                        datas.Enqueue(data);
                    }
                }
            }

        }

        private void DoWorkAccumulation()
        {

            try
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                //var list = datas.Where(a => a.enumEtwStatus == EnumEtwStatus.Processed).Take(accumulationSecond).ToList();
                //if (list != null && list.Count == accumulationSecond)
                //{
                //    AnalyzeAccumulation(list);
                //    stopwatch.Stop();
                //    logger.Debug(JsonConvert.SerializeObject(new
                //    {
                //        time = stopwatch.Elapsed.TotalMilliseconds
                //    }));
                //    foreach (EtwData etwData in list)
                //    {
                //        var etw = etwData;
                //        datas.TryTake(out etw);
                //    }
                //}


                var list = handledDatas.Where(a => a.enumEtwStatus == EnumEtwStatus.Processed)
                                .GroupBy(a => a.senderIp)
                                .Where(g => g.Count() >= accumulationSecond);

                var enumerable = list as IGrouping<string, EtwData>[] ?? list.ToArray();
                if (enumerable.Any())
                {
                    foreach (var gp in enumerable)
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        var dataList = gp.OrderBy(a=>a.time).Take(accumulationSecond).ToList();
                        AnalyzeAccumulation(dataList);
                        stopwatch.Stop();
                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    time = stopwatch.Elapsed.TotalMilliseconds
                        //}));
                        foreach (EtwData etwData in dataList)
                        {
                            handledDatas.Remove(etwData);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        private void Analyze(EtwData data)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (data != null)
            {
                //数据解析
                List<CloudflareLog> cloudflareLogs = ParseEtwData(data);
                stopwatch.Stop();
                //logger.Info(JsonConvert.SerializeObject(new
                //{
                //    timeType = "ParseEtwData",
                //    time = stopwatch.Elapsed.TotalMilliseconds
                //}));
                stopwatch.Restart();
                //分析
                AnalyzeResult analyzeResult = AnalyzeRatelimit(cloudflareLogs);
                stopwatch.Stop();
                //logger.Info(JsonConvert.SerializeObject(new
                //{
                //    timeType = "AnalyzeRatelimit",
                //    time = stopwatch.Elapsed.TotalMilliseconds
                //}));
                stopwatch.Restart();
                //发送结果
                SendResult(analyzeResult);
                stopwatch.Stop();
                //logger.Info(JsonConvert.SerializeObject(new
                //{
                //    timeType = "SendResult",
                //    time = stopwatch.Elapsed.TotalMilliseconds
                //}));

                handledDatas.Add(data);
            }
        }

        private void AnalyzeAccumulation(List<EtwData> dataList)
        {
            if (dataList != null)
            {
                List<CloudflareLog> cloudflareLogs = new List<CloudflareLog>();
                foreach (EtwData etwData in dataList)
                {
                    //数据解析
                    //cloudflareLogs.AddRange(ParseEtwData(etwData));
                    cloudflareLogs.AddRange(etwData.parsedData);
                }
                //分析
                AnalyzeResult analyzeResult = AnalyzeRatelimit(cloudflareLogs, accumulationSecond);
                //发送结果
                SendResult(analyzeResult);
            }
        }

        private List<CloudflareLog> ParseEtwData(EtwData data)
        {
            List<CloudflareLog> cloudflareLogs = new List<CloudflareLog>();
            //ConcurrentBag<CloudflareLog> cloudflareLogsBag = new ConcurrentBag<CloudflareLog>();
            if (data != null)
            {
                foreach (var buff in data.buffList)
                {
                    ETWPrase eTWPrase = new ETWPrase(buff);

                    if (!parseEtwDataLog)
                    {
                        logger.Debug(JsonConvert.SerializeObject(eTWPrase));
                        parseEtwDataLog = true; //只打印一条明细ETW 日志
                    }

                    cloudflareLogs.Add(new CloudflareLog
                    {
                        ClientRequestHost = eTWPrase.Cs_host,
                        ClientIP = !string.IsNullOrEmpty(eTWPrase.CFConnectingIP) ? eTWPrase.CFConnectingIP : eTWPrase.C_ip,
                        ClientRequestURI = $"{eTWPrase.Cs_uri_stem}?{eTWPrase.cs_uri_query}",
                        ClientRequestMethod = eTWPrase.Cs_method,
                    });
                }
                //var list = data.buffList.ToList();
                //Parallel.For(0, list.Count(), index =>
                //{
                //    var buff = list[index];
                //    ETWPrase eTWPrase = new ETWPrase(buff);
                //    cloudflareLogsBag.Add(new CloudflareLog
                //    {
                //        ClientRequestHost = eTWPrase.Cs_host,
                //        ClientIP = !string.IsNullOrEmpty(eTWPrase.CFConnectingIP) ? eTWPrase.CFConnectingIP : eTWPrase.C_ip,
                //        ClientRequestURI = string.Format("{0}?{1}", eTWPrase.Cs_uri_stem, eTWPrase.cs_uri_query),
                //        ClientRequestMethod = eTWPrase.Cs_method,
                //    });
                //});
            }
            //cloudflareLogs = cloudflareLogsBag.ToList();
            if (data != null) data.parsedData = cloudflareLogs;
            return cloudflareLogs;
        }

        private AnalyzeResult AnalyzeRatelimit(List<CloudflareLog> cloudflareLogs,int accumulation=1)
        {
            AnalyzeResult analyzeResult = new AnalyzeResult();
            if (cloudflareLogs != null)
            {
                //logger.Debug(JsonConvert.SerializeObject(new
                //{
                //    DataType = "0-CloudflareLogs",
                //    Value = cloudflareLogs,
                //}));

                var key = "AnalyzeRatelimit_GetZoneList_Key";
                List<ZoneEntity> zoneEntityList = Utils.GetMemoryCache(key, () =>
                {
                    string url = awsGetZoneListApiUrl;
                    string content = HttpGet(url);
                    //logger.Debug(JsonConvert.SerializeObject(new
                    //{
                    //    DataType = "GetZoneList",
                    //    Value = content,
                    //}));
                    return JsonConvert.DeserializeObject<List<ZoneEntity>>(content);
                }, 1440);
                
                CloudflareLog cloudflare = cloudflareLogs.FirstOrDefault();
                string zoneId = zoneEntityList.FirstOrDefault(a => (a.HostNames.Split(new string[] { Utils.Separator }, StringSplitOptions.RemoveEmptyEntries)).Contains(cloudflare.ClientRequestHost))?.ZoneId;//?

                //logger.Debug(JsonConvert.SerializeObject(new
                //{
                //    FirstOrDefault = cloudflare,
                //    ZoneId = zoneId,
                //}));

                if (!string.IsNullOrEmpty(zoneId))
                {
                    //每60分钟去获取一次RateLimit规则
                    key = "AnalyzeRatelimit_GetRatelimits_Key_" + zoneId;
                    List<RateLimitEntity> rateLimitEntities = Utils.GetMemoryCache(key, () =>
                    {
                        string url = getRatelimitsApiUrl;
                        url = url.Replace("{zoneId}", zoneId);
                        string content = HttpGet(url);
                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    DataType = "GetRatelimits",
                        //    ZoneId = zoneId,
                        //    Value = content,
                        //}));
                        return JsonConvert.DeserializeObject<List<RateLimitEntity>>(content);
                    }, 5);

                    key = "AnalyzeRatelimit_GetWhiteList_Key_" + zoneId;
                    List<WhiteListModel> whiteListModels = Utils.GetMemoryCache(key, () =>
                    {
                        string url = awsGetWhiteListApiUrl;
                        url = url.Replace("{zoneId}", zoneId);
                        string content = HttpGet(url);
                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    DataType = "GetWhiteList",
                        //    ZoneId = zoneId,
                        //    Value = content,
                        //}));
                        return JsonConvert.DeserializeObject<List<WhiteListModel>>(content);
                    }, 1440);

                    //获取1S规则
                    List<RateLimitEntity> rateLimitEntitiesSub = rateLimitEntities.Where(a => a.ZoneId == zoneId && a.Period == 1).ToList();
                    if (accumulation > 1)
                    {
                        rateLimitEntitiesSub = rateLimitEntities.Where(a => a.ZoneId == zoneId && a.Period > 1).ToList();
                    }

                    if (rateLimitEntitiesSub != null && rateLimitEntitiesSub.Count > 0)
                    {
                        List<string> ipWhiteList = new List<string>();
                        if (whiteListModels != null)
                        {
                            ipWhiteList = whiteListModels.Select(a => a.IP).ToList();
                        }

                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    DataType = "1-CloudflareLogs",
                        //    Value = cloudflareLogs,
                        //}));

                        var logAnalyzeModelList = cloudflareLogs.Where(a => !ipWhiteList.Contains(a.ClientIP))
                            .Select(x =>
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
                            });

                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    DataType = "2-CloudflareLogs-!whitelist",
                        //    Value = logAnalyzeModelList,
                        //}));

                        var itemsGroup = logAnalyzeModelList.GroupBy(a => new { a.IP, a.RequestHost, a.RequestUrl })
                                            .Select(g => new LogAnalyzeModel
                                            {
                                                RequestHost = g.Key.RequestHost,
                                                IP = g.Key.IP,
                                                RequestUrl = g.Key.RequestUrl,
                                                RequestCount = g.Count()
                                            }).ToList();

                        var result =
                            (from analyzeModel in itemsGroup
                             from config in rateLimitEntitiesSub
                             where IfMatchCondition(config, analyzeModel)
                             select new LogAnalyzeModel
                             {
                                 RequestHost = analyzeModel.RequestHost,
                                 IP = analyzeModel.IP,
                                 RequestUrl = analyzeModel.RequestUrl,
                                 RequestCount = analyzeModel.RequestCount,
                                 RateLimitId = config.ID,

                             });

                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    DataType = "3-CloudflareLogs-IfMatchCondition",
                        //    Value = result,
                        //}));

                        List<Result> results = new List<Result>();
                        if (result != null && result.Count() > 0)
                        {
                            foreach (var item in result)
                            {
                                RateLimitEntity rateLimit = rateLimitEntitiesSub.FirstOrDefault(a => a.ID == item.RateLimitId);

                                int ruleId = rateLimit.ID;
                                int period = rateLimit.Period;
                                int threshold = rateLimit.Threshold;
                                string url = rateLimit.Url;
                                var enlargementFactor = rateLimit.EnlargementFactor;

                                //触发规则的 IP+Host 数据
                                List<LogAnalyzeModel> logAnalyzesList = logAnalyzeModelList.Where(a => a.RequestHost == item.RequestHost && a.IP == item.IP)
                                    .GroupBy(a => a.RequestFullUrl)
                                    .Select(g => new LogAnalyzeModel
                                    {
                                        RequestFullUrl = g.Key,
                                        RequestCount = g.Count(),
                                    }).ToList();

                                Result rst = results.FirstOrDefault(a => a.RuleId == ruleId);
                                if (rst == null)
                                {
                                    List<BrokenIp> brokenIpList = new List<BrokenIp>();
                                    rst = new Result
                                    {
                                        RuleId = ruleId,
                                        Period = period,
                                        Threshold = threshold,
                                        EnlargementFactor = enlargementFactor,
                                        Url = url,
                                        BrokenIpList = brokenIpList,
                                    };
                                    results.Add(rst);
                                }
                                BrokenIp brokenIp = new BrokenIp
                                {
                                    IP = item.IP,
                                    RequestRecords = new List<RequestRecord>(),
                                };
                                rst.BrokenIpList.Add(brokenIp);
                                if (logAnalyzesList != null)
                                {
                                    foreach (var logAnalyze in logAnalyzesList)
                                    {
                                        brokenIp.RequestRecords.Add(new RequestRecord
                                        {
                                            FullUrl = logAnalyze.RequestFullUrl,
                                            RequestCount = logAnalyze.RequestCount,
                                        });
                                    }
                                }
                            }
                        }
                        analyzeResult.ZoneId = zoneId;
                        analyzeResult.timeStage = accumulation;
                        analyzeResult.result = results;

                        //logger.Debug(JsonConvert.SerializeObject(new
                        //{
                        //    DataType = "4-CloudflareLogs-analyzeResult",
                        //    Value = analyzeResult,
                        //}));
                    }
                }
            }
            return analyzeResult;
        }

        private void SendResult(AnalyzeResult analyzeResult)
        {
            if(analyzeResult!=null&& analyzeResult.result!=null&& analyzeResult.result.Count > 0)
            {
                Task.Run(() => {
                    string url = analyzeResultApiUrl;
                    string json = JsonConvert.SerializeObject(analyzeResult);
                    HttpPost(url, json);
                });
            }
        }

        private string HttpGet(string url, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authKey);
                string strResult = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }

        private string HttpPost(string url, string json, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                string strResult = "";
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authKey);
                strResult = client.PostAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }

        private bool IfMatchCondition(RateLimitEntity rateLimit, LogAnalyzeModel analyzeModel)
        {
            bool bResult = false;
            bResult = rateLimit.Url.EndsWith("*") ? ((analyzeModel.RequestUrl.ToLower().StartsWith(rateLimit.Url.Replace("*", "").ToLower())) && (analyzeModel.RequestCount >= rateLimit.Threshold))
                : ((analyzeModel.RequestUrl.ToLower().Equals(rateLimit.Url.ToLower())) && (analyzeModel.RequestCount >= rateLimit.Threshold));
            return bResult;
        }

        private bool IfMatchConditionAccumulation(RateLimitEntity rateLimit, LogAnalyzeModel analyzeModel)
        {
            bool bResult = false;
            bResult = rateLimit.Url.EndsWith("*") ? ((analyzeModel.RequestUrl.ToLower().StartsWith(rateLimit.Url.Replace("*", "").ToLower())) && (analyzeModel.RequestCount >= rateLimit.Threshold * accumulationSecond / (float)rateLimit.Period))
                : ((analyzeModel.RequestUrl.ToLower().Equals(rateLimit.Url.ToLower())) && (analyzeModel.RequestCount >= rateLimit.Threshold * accumulationSecond / (float)rateLimit.Period));
            return bResult;
        }
    }
}
