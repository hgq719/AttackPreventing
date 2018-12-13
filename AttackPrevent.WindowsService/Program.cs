using AttackPrevent.Business;
using AttackPrevent.WindowsService.Job;
using AttackPrevent.WindowsService.SysConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Linq;
using Quartz;
using Quartz.Impl;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Xunit;
using Shouldly;

namespace AttackPrevent.WindowsService
{
    public class Program
    {

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("AttackPrevent.WindowsService.exe.config"));
            //Test();
            RunProgram().GetAwaiter().GetResult();
            //Console.ReadKey();

            while (true)
            {
                var sleepTime = 1000;
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    Console.WriteLine(
                        $"[app] [{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss}] Start to get logs and analysis data.");

                    new LogAnalyzeJob().Execute(null);
                    sw.Stop();

                    Console.WriteLine(
                        $"[app] [{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss}] Finished to get logs and analysis data, time elapsed：{sw.ElapsedMilliseconds/1000} seconds.");
                    sleepTime = 2*60*1000 - (int)sw.ElapsedMilliseconds;
                }
                catch (Exception ex)
                {
                    //Code review by michael. 1. 只有错误信息，没有堆栈. 2. 报错信息记录在哪里? 又没有日志，以后线上报错怎么查? 
                    Console.Write(ex.Message);
                }
                
                Thread.Sleep(sleepTime > 0 ? sleepTime: 1000);
            }
            //new LogAnalyzeJob().Execute(null);
            //return;
            if (args.Length != 0)
            {
                WindowsServiceInstaller srv = new WindowsServiceInstaller();
                switch (args[0].ToLower())
                {
                    case "/install":
                        Install(srv);
                        return;
                    case "/uninstall":
                        Uninstall(srv);
                        return;
                    case "/debug":
                        Debug();
                        return;
                    default:
                        Help();
                        return;
                }
            }
            else
            {
                Start();
            }
        }

        #region 服务相关
        private const string SrvName = "Comm100.AcctackLogAnalyzeService";
        private const string SrvDisplay = "Comm100.AcctackLogAnalyzeService";
        //Code review by michael (string not String, s 是小写的）
        private const String SrvDescription = "Acctack Log Analyze windodws Service for Comm100";
        #endregion

        #region 方法定义
        private static void Install(WindowsServiceInstaller srv)
        {
            bool ok = srv.InstallService(System.Reflection.Assembly.GetExecutingAssembly().Location, SrvName, SrvDisplay, SrvDescription);

            if (ok)
                Console.WriteLine("Service installed.");
            else
                Console.WriteLine("There was a problem with installation.");
        }

        private static void Uninstall(WindowsServiceInstaller srv)
        {
            var ok = srv.UnInstallService(SrvDisplay);

            // Code review by michael Console.WriteLine(ok ? "Service uninstalled." : "There was a problem with uninstallation.");
            if (ok)
                Console.WriteLine("Service uninstalled.");
            else
                Console.WriteLine("There was a problem with uninstallation.");
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
        #endregion

        #region Test
        public static void Test()
        {
            // Test
            List<string> urls = new List<string>() {
                "ent.comm100.com/LiveChathandler3.ashx?siteId=1000234 (Avg: 954)",
                "ent.comm100.com/FileUploadHandler.ashx?siteId=1000234 (Avg: 634)",
                "ent.comm100.com/errorcollector.ashx?siteId=1000234 (Avg: 456)"
            };
            string json = JsonConvert.SerializeObject(urls);
            Model.ActionReport actionReport = new Model.ActionReport
            {
                IP = "xx.xx.xx.xx",
                HostName = "comm100.com",
                Max = 2000,
                Min = 500,
                Avg = 600,
                Count = 100000,
                Mode = "WhiteList",
                MaxDisplay = "2000(1200)",
                MinDisplay = "500(300)",
                AvgDisplay = "600(400)",
                CreatedTime = DateTime.Now,
                Title = "06/12/2018",
                ZoneId = "2068c8964a4dcef78ee5103471a8db03",
                FullUrl = json,
                Remark = "test",
            };
            ActionReportBusiness.Add(actionReport);
            List<Model.ActionReport> reportList = ActionReportBusiness.GetListByIp("xx.xx.xx.xx");
            actionReport = ActionReportBusiness.GetListByTitle("06/12/2018").FirstOrDefault();
            actionReport.Mode = "Action";
            int maxWhiteList = ActionReportBusiness.GetMaxForWhiteList("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");
            int minWhiteList = ActionReportBusiness.GetMinForWhiteList("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");
            int avgWhiteList = ActionReportBusiness.GetAvgForWhiteList("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");

            ActionReportBusiness.Edit(actionReport);

            int maxAction = ActionReportBusiness.GetMaxForAction("2068c8964a4dcef78ee5103471a8db03","xx.xx.xx.xx", "comm100.com");
            int minAction = ActionReportBusiness.GetMinForAction("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");
            int avgAction = ActionReportBusiness.GetAvgForAction("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");

            ActionReportBusiness.Delete("06/12/2018");

            Model.SmtpQueue smtpQueue = new Model.SmtpQueue
            {
                Title = "06/12/2018",
                Status = 1,
                CreatedTime = DateTime.Now,
                SendedTime = DateTime.Now,
                Remark = "test",
            };
            SmtpQueueBusiness.Add(smtpQueue);
            List<Model.SmtpQueue> queueList = SmtpQueueBusiness.GetList();
            smtpQueue = SmtpQueueBusiness.GetByTitle("06/12/2018");
            smtpQueue.Status = 0;
            SmtpQueueBusiness.Edit(smtpQueue);
            SmtpQueueBusiness.Delete("06/12/2018");

            IActiveReportService activeReportService = ActiveReportService.GetInstance();
            activeReportService.GeneratedActiveReport();

            ISendMailService sendMailService = SendMailService.GetInstance();
            sendMailService.MainQueueDoWork();

            ConstValues.emailtimeout.ShouldBe(90000);
        }
        #endregion

        #region Start Quartz
        static async Task RunProgram()
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();
            }
            catch(Exception e)
            {
                await Console.Error.WriteLineAsync(e.ToString());
            }
        }
        #endregion
    }
}
