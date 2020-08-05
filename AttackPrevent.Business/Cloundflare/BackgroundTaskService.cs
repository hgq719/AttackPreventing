using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business.Cloundflare
{
    public interface IBackgroundTaskService
    {
        string Enqueue(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end);
        GetCloundflareLogsBackgroundInfo GetOperateStatus(string guid);
        List<CloudflareLog> GetCloudflareLogs(string guid, int limit, int offset, string host, string siteId, string url, string cacheStatus, string ip, string responseStatus);
        void doWork();
        int GetTotal(string guid, string host, string siteId, string url, string cacheStatus, string ip, string responseStatus);
        List<CloudflareLog> GetCloudflareLogs(string guid, string host, string siteId, string url, string cacheStatus, string ip, string responseStatus);
    }
    public class BackgroundTaskService: IBackgroundTaskService
    {
        private ConcurrentQueue<GetCloundflareLogsBackgroundInfo> backgroundInfos;
        private static object obj_Sync = new object();
        private static BackgroundTaskService backgroundTaskService;
        private ILogService logger = new LogService();
        private bool ifBusy = false;

        private BackgroundTaskService()
        {
            backgroundInfos = new ConcurrentQueue<GetCloundflareLogsBackgroundInfo>();
        }
        public static BackgroundTaskService GetInstance()
        {
            if(backgroundTaskService == null)
            {
                lock(obj_Sync)
                {
                    if(backgroundTaskService == null)
                    {
                        backgroundTaskService = new BackgroundTaskService();
                    }
                }
            }
            return backgroundTaskService;
        }

        public string Enqueue(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end)
        {
            string key = string.Empty;
            if ( Convert.ToDateTime( end.ToString("yyyy-MM-dd HH:mm") ) > Convert.ToDateTime(start.ToString("yyyy-MM-dd HH:mm")))
            {
                key = string.Format("{0}-{1}-{2}-{3}", start.ToString("yyyyMMddHHmmss"), end.ToString("yyyyMMddHHmmss"), sample, zoneId);
                if (Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(key) != null)
                {

                }
                else
                {
                    GetCloundflareLogsBackgroundInfo backgroundInfo = new GetCloundflareLogsBackgroundInfo
                    {
                        Guid = key,
                        ZoneId = zoneId,
                        AuthEmail = authEmail,
                        AuthKey = authKey,
                        Sample = sample,
                        StartTime = start,
                        EndTime = end,
                        Status = EnumBackgroundStatus.Processing,
                        CloudflareLogs = new List<CloudflareLog>(),

                    };
                    backgroundInfos.Enqueue(backgroundInfo);
                    Utils.SetMemoryCache(backgroundInfo.Guid, backgroundInfo);
                }
            }          
            
            return key;
        }

        public void doWork()
        {
            if (!ifBusy)
            {
                string key = "";
                GetCloundflareLogsBackgroundInfo backgroundInfo = null;
                try
                {
                    ifBusy = true;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
    
                    if (backgroundInfos.TryDequeue(out backgroundInfo))
                    {
                        double sample = backgroundInfo.Sample;
                        DateTime startTime = backgroundInfo.StartTime;
                        DateTime endTime = backgroundInfo.EndTime;
                        string zoneId = backgroundInfo.ZoneId;
                        string authEmail = backgroundInfo.AuthEmail;
                        string authKey = backgroundInfo.AuthKey;
                        key = string.Format("{0}-{1}-{2}-{3}", startTime.ToString("yyyyMMddHHmmss"), endTime.ToString("yyyyMMddHHmmss"), sample, zoneId);

                        ICloudflareLogHandleSercie cloudflareLogHandleSercie = new CloudflareLogHandleSercie(zoneId, authEmail, authKey, sample, startTime, endTime);
                        cloudflareLogHandleSercie.TaskStart();
                        List<CloudflareLog> cloudflareLogs = cloudflareLogHandleSercie.GetCloudflareLogs(key);
                        backgroundInfo.CloudflareLogs = cloudflareLogs;
                        backgroundInfo.Status = EnumBackgroundStatus.Succeeded;
                        Utils.SetMemoryCache(backgroundInfo.Guid, backgroundInfo);

                        stopwatch.Stop();

                    }
                    ifBusy = false;
                }
                catch(Exception e)
                {
                    logger.Error(e);
                    if(!string.IsNullOrEmpty(key) &&
                        backgroundInfo != null)
                    {
                        backgroundInfo.Status = EnumBackgroundStatus.Failed;
                        backgroundInfo.ErrorMessage = string.IsNullOrEmpty(e.InnerException?.Message) ? e.Message : e.InnerException?.Message;
                        Utils.SetMemoryCache(backgroundInfo.Guid, backgroundInfo);
                    }
            
                    ifBusy = false;
                }                
            }
        }

        public GetCloundflareLogsBackgroundInfo GetOperateStatus(string guid)
        {
            //EnumBackgroundStatus enumBackgroundStatus = EnumBackgroundStatus.Failed;
            GetCloundflareLogsBackgroundInfo backgroundInfo = new GetCloundflareLogsBackgroundInfo { Status = EnumBackgroundStatus.Failed, };
            GetCloundflareLogsBackgroundInfo backgroundInfoCache = Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(guid);
            if (backgroundInfoCache != null)
            {
                backgroundInfo = backgroundInfoCache;
            }
            return backgroundInfo;
        }

        public List<CloudflareLog> GetCloudflareLogs(string guid, int limit, int offset, string host, string siteId, string url, string cacheStatus, string ip, string responseStatus)
        {
            List<CloudflareLog> cloudflareLogs = new List<CloudflareLog>();
            GetCloundflareLogsBackgroundInfo backgroundInfo = Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(guid);                
            if (backgroundInfo!=null && backgroundInfo.Status == EnumBackgroundStatus.Succeeded)
            {
                var query = backgroundInfo.CloudflareLogs.AsQueryable();
                if (!string.IsNullOrEmpty(host))
                {
                    query = query.Where(a => a.ClientRequestHost.Contains(host));
                }
                if (!string.IsNullOrEmpty(siteId))
                {
                    query = query.Where(a => a.ClientRequestURI.Contains(string.Format("siteId={0}",siteId)));
                }
                if (!string.IsNullOrEmpty(url))
                {
                    query = query.Where(a => a.ClientRequestURI.Contains(string.Format("{0}", url)));
                }
                if (!string.IsNullOrEmpty(cacheStatus) && cacheStatus.ToLower() != "all")
                {
                    query = query.Where(a => a.CacheCacheStatus == string.Format("{0}", cacheStatus));
                }
                if (!string.IsNullOrEmpty(ip))
                {
                    query = query.Where(a => a.ClientIP == string.Format("{0}", ip));
                }
                if (!string.IsNullOrEmpty(responseStatus))
                {
                    query = query.Where(a => a.EdgeResponseStatus == int.Parse(responseStatus));
                }
                cloudflareLogs = query.Skip(offset).Take(limit).ToList();
            }
            return cloudflareLogs;
        }

        public int GetTotal(string guid, string host, string siteId, string url, string cacheStatus, string ip, string responseStatus)
        {
            int total = 0;
            GetCloundflareLogsBackgroundInfo backgroundInfo = Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(guid);
            if (backgroundInfo != null && backgroundInfo.Status == EnumBackgroundStatus.Succeeded)
            {
                var query = backgroundInfo.CloudflareLogs.AsQueryable();
                if (!string.IsNullOrEmpty(host))
                {
                    query = query.Where(a => a.ClientRequestHost.Contains(host));
                }
                if (!string.IsNullOrEmpty(siteId))
                {
                    query = query.Where(a => a.ClientRequestURI.Contains(string.Format("siteId={0}", siteId)));
                }
                if (!string.IsNullOrEmpty(url))
                {
                    query = query.Where(a => a.ClientRequestURI.Contains(string.Format("{0}", url)));
                }
                if (!string.IsNullOrEmpty(cacheStatus) && cacheStatus.ToLower() != "all")
                {
                    query = query.Where(a => a.CacheCacheStatus == string.Format("{0}", cacheStatus));
                }
                if (!string.IsNullOrEmpty(ip))
                {
                    query = query.Where(a => a.ClientIP == string.Format("{0}", ip));
                }
                if (!string.IsNullOrEmpty(responseStatus))
                {
                    query = query.Where(a => a.EdgeResponseStatus == int.Parse(responseStatus));
                }
                total = query.Count();
            }
            return total;
        }

        public List<CloudflareLog> GetCloudflareLogs(string guid, string host, string siteId, string url, string cacheStatus, string ip, string responseStatus)
        {
            List<CloudflareLog> cloudflareLogs = new List<CloudflareLog>();
            GetCloundflareLogsBackgroundInfo backgroundInfo = Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(guid);
            if (backgroundInfo != null && backgroundInfo.Status == EnumBackgroundStatus.Succeeded)
            {
                var query = backgroundInfo.CloudflareLogs.AsQueryable();
                if (!string.IsNullOrEmpty(host))
                {
                    query = query.Where(a => a.ClientRequestHost.Contains(host));
                }
                if (!string.IsNullOrEmpty(siteId))
                {
                    query = query.Where(a => a.ClientRequestURI.Contains(string.Format("siteId={0}", siteId)));
                }
                if (!string.IsNullOrEmpty(url))
                {
                    query = query.Where(a => a.ClientRequestURI.Contains(string.Format("{0}", url)));
                }
                if (!string.IsNullOrEmpty(cacheStatus)&& cacheStatus.ToLower()!="all")
                {
                    query = query.Where(a => a.CacheCacheStatus == string.Format("{0}", cacheStatus));
                }
                if (!string.IsNullOrEmpty(ip))
                {
                    query = query.Where(a => a.ClientIP == string.Format("{0}", ip));
                }
                if (!string.IsNullOrEmpty(responseStatus))
                {
                    query = query.Where(a => a.EdgeResponseStatus == int.Parse(responseStatus));
                }
                cloudflareLogs = query.ToList();
            }
            return cloudflareLogs;
        }
    }
}
