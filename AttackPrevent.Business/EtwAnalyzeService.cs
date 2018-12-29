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
    public interface IEtwAnalyzeService
    {
        void Add(List<byte[]> data);
        void doWork();
    }
    public class EtwAnalyzeService : IEtwAnalyzeService
    {
        private static IEtwAnalyzeService etwAnalyzeService;
        private static object obj_Sync = new object();
        private ConcurrentQueue<List<byte[]>> datas;
        private ILogService logger = new LogService();
        private EtwAnalyzeService()
        {
            datas = new ConcurrentQueue<List<byte[]>>();
        }
        public static IEtwAnalyzeService GetInstance()
        {
            if (etwAnalyzeService == null)
            {
                lock (obj_Sync)
                {
                    if (etwAnalyzeService == null)
                    {
                        etwAnalyzeService = new EtwAnalyzeService();
                    }
                }
            }
            return etwAnalyzeService;
        }

        public void Add(List<byte[]> data)
        {
            datas.Enqueue(data);
        }

        public void doWork()
        {
            while (true)
            {
                List<byte[]> data = null;
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    if (datas.TryDequeue(out data))
                    {
                        Analyze(data);
                        stopwatch.Stop();
                        logger.Debug(stopwatch.Elapsed.TotalMilliseconds);
                    }

                }
                catch(Exception e)
                {
                    logger.Error(e.StackTrace);
                    if (data != null)
                    {
                        Add(data);
                    }
                }
                finally
                {

                }
            }
        }
        private void Analyze(List<byte[]> data)
        {
            if (data != null)
            {

            }
        }

    }
}
