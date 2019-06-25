using AttackPrevent.Business;
using AttackPrevent.WindowsService.Job;
using AttackPrevent.WindowsService.SysConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Linq;
using Quartz;
using Quartz.Impl;
using System.Threading.Tasks;
using Shouldly;
using log4net.Config;
using System.Text;
using AttackPrevent.Model;
using System.Collections.Concurrent;

namespace AttackPrevent.WindowsService
{
    public class Program
    {
        private static readonly ILogService LogService = new LogService();
        static void Main()
        {
            try
            {
                XmlConfigurator.Configure(new System.IO.FileInfo("AttackPrevent.WindowsService.exe.config"));
                var timer = new System.Threading.Timer(new TimerCallback(timer_Elapsed), null, 0, 2 * 60 * 1000);
                Console.ReadLine();
                timer.Dispose();
            }
            catch (Exception ex)
            {
                var msg = $" error message = {ex.Message}. \n stack trace = {ex.StackTrace}";
                LogService.Error(msg);
            }
        }

        private static void timer_Elapsed(object sender)
        {
            try
            {
                var job = new LogAnalyzeJob();
                job.Execute();
            }
            catch (Exception ex)
            {
                var msg = $" error message = {ex.Message}. \n stack trace = {ex.StackTrace}";
                LogService.Error(msg);
            }
        }
        
        private const string SrvName = "Comm100.AcctackLogAnalyzeService";
        private const string SrvDisplay = "Comm100.AcctackLogAnalyzeService";
        private const string SrvDescription = "Acctack Log Analyze windodws Service for Comm100";
       

      

        private static void Uninstall(WindowsServiceInstaller srv)
        {
            Console.WriteLine(srv.UnInstallService(SrvDisplay) ? "Service uninstalled." : "There was a problem with uninstallation.");
        }


        private static void Debug()
        {
            new AnalyzeService().Start();
            Console.ReadLine();
        }

        private static void Help()
        {
            Console.WriteLine("Command line parameters:");

            Console.WriteLine("\t/install\tInstalls the service");

            Console.WriteLine("\t/uninstall\tUnstalls the service");

            Console.WriteLine("\t/debug\tDebug the service");

            Console.ReadLine();
        }

        private static void Start()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                    new AnalyzeService()
            };
            ServiceBase.Run(ServicesToRun);
        }
      

        #region Test
        public static void TestIIsLog()
        {

            IAttackPreventService attackPreventService = AttackPreventService.GetInstance();
            AnalyzeResult analyzeResult = new AnalyzeResult {
               ZoneId= "2068c8964a4dcef78ee5103471a8db03",
               timeStage=1,
               result=new List<Result> {
                 new Result{
                     Url="test.comm100.com",
                     Threshold=1,
                     Period=1,
                     EnlargementFactor=1,
                     RuleId=1,
                     BrokenIpList=new List<BrokenIp>{
                        new BrokenIp{
                            IP="0.0.0.0",
                            RequestRecords=new List<RequestRecord>{
                                new RequestRecord{
                                    FullUrl="test.comm100.com?queryString=hello",
                                    RequestCount=100,
                                }
                            }
                        }
                     }
                 }
               },
            };
            Parallel.For(0, 3, index =>
            {
                attackPreventService.Add(analyzeResult);
            });

            IEtwAnalyzeService etwAnalyzeService = EtwAnalyzeService.GetInstance();
            byte[] buff = Encoding.UTF8.GetBytes("hello world");
            ConcurrentBag<byte[]> data = new ConcurrentBag<byte[]>() {
                buff
            };
            string ip = "0.0.0.0";
            etwAnalyzeService.Add(ip,data);
            etwAnalyzeService.DoWork();

            Console.ReadLine();
        }
        #endregion
    }
}
