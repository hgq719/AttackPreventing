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
        EnumBackgroundStatus GetOperateStatus(string guid);
        List<CloudflareLog> GetCloudflareLogs(string guid, int pageSize, int pageIndex);
        void doWork();
    }
    public class BackgroundTaskService: IBackgroundTaskService
    {
        private ConcurrentQueue<GetCloundflareLogsBackgroundInfo> backgroundInfos;
        private static object obj_Sync = new object();
        private static BackgroundTaskService backgroundTaskService;

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
                    backgroundTaskService = new BackgroundTaskService();
                }
            }
            return backgroundTaskService;
        }

        public string Enqueue(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end)
        {
            GetCloundflareLogsBackgroundInfo backgroundInfo = new GetCloundflareLogsBackgroundInfo
            {
                Guid = Guid.NewGuid().ToString(),
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
            return backgroundInfo.Guid;
        }

        public void doWork()
        {
            while (true)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                GetCloundflareLogsBackgroundInfo backgroundInfo;
                if (backgroundInfos.TryDequeue(out backgroundInfo))
                {
                    double sample = 0.01;
                    DateTime startTime= backgroundInfo.StartTime;
                    DateTime endTime= backgroundInfo.EndTime;
                    string zoneId= backgroundInfo.ZoneId;
                    string authEmail= backgroundInfo.AuthEmail;
                    string authKey= backgroundInfo.AuthKey;

                    ICloudflareLogHandleSercie cloudflareLogHandleSercie = new CloudflareLogHandleSercie(zoneId, authEmail, authKey, sample, startTime, endTime);
                    cloudflareLogHandleSercie.TaskStart();
                    List<CloudflareLog> cloudflareLogs = cloudflareLogHandleSercie.GetCloudflareLogs();
                    backgroundInfo.CloudflareLogs = cloudflareLogs;
                    backgroundInfo.Status = EnumBackgroundStatus.Succeeded;
                    Utils.SetMemoryCache(backgroundInfo.Guid, backgroundInfo);
                    
                    stopwatch.Stop();

                }
            }
        }

        public EnumBackgroundStatus GetOperateStatus(string guid)
        {
            return Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(guid).Status;
        }

        public List<CloudflareLog> GetCloudflareLogs(string guid, int pageSize, int pageIndex)
        {
            List<CloudflareLog> cloudflareLogs = new List<CloudflareLog>();
            GetCloundflareLogsBackgroundInfo backgroundInfo = Utils.GetMemoryCache<GetCloundflareLogsBackgroundInfo>(guid);
            if (backgroundInfo.Status == EnumBackgroundStatus.Succeeded)
            {
                int count = (pageIndex - 1) * pageSize;
                cloudflareLogs = backgroundInfo.CloudflareLogs.Skip(count).Take(pageSize).ToList();
            }
            return cloudflareLogs;
        }
    }
}
