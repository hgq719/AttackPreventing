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
            if (analyzeResult != null&& analyzeResult.result!=null)
            {
                //记录日志
                InsertLogs(analyzeResult);
                //开启RateLimit
                OpenRageLimit(analyzeResult);
                //Ban IP
                BanIp(analyzeResult);
            }
        }
        private void InsertLogs(AnalyzeResult analyzeResult)
        {

        }
        private void OpenRageLimit(AnalyzeResult analyzeResult)
        {

        }
        private void BanIp(AnalyzeResult analyzeResult)
        {
            IBlackListBusinees blackListBusinees = new BlackListBusinees();
            var zoneList = ZoneBusiness.GetZoneList();
            string zoneID = analyzeResult.ZoneId;
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            string authEmail = zone.AuthEmail;
            string authKey = zone.AuthKey;
            string comment = "Add BlackList by iis logs";

            foreach (var rst in analyzeResult.result)
            {
                foreach (var broken in rst.BrokenIpList)
                {
                    string ip = broken.IP;
                    bool flag = blackListBusinees.CreateAccessRule(zoneID, authEmail, authKey, ip, comment);
                    if (!flag)
                    {
                        logger.Error("Add BlackList by iis logs fail, ip=" + ip);
                    }
                }
            }
        }

    }
}
