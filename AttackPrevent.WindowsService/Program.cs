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
                RunProgram().GetAwaiter().GetResult();
                var timer = new System.Threading.Timer(new TimerCallback(timer_Elapsed), null, 0, 2*60*1000);
                Console.ReadLine();
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
        //Code review by michael (string not String, s 是小写的）
        private const String SrvDescription = "Acctack Log Analyze windodws Service for Comm100";
       

      

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
      

        #region Test
        public static void Test()
        {
            // Test
            //DateTime startTime = new DateTime(2018, 12, 14, 0, 0, 0);
            //DateTime endTime = new DateTime(2018, 12, 14, 0, 1, 0);
            //double sample = 0.01d;
            //string zoneId = "2068c8964a4dcef78ee5103471a8db03";
            //string authEmail = "cloudflareapidep@comm100.com";
            //string authKey = "Bh4yzL0DRq5WFhawU3FmdD6OjQ5DLY5tmg3gSbLJDObu8rGR4yKvvngn8pGDhn2d";
            //string key = string.Format("{0}-{1}-{2}-{3}", startTime.ToString("yyyyMMddHHmmss"), endTime.ToString("yyyyMMddHHmmss"), sample, zoneId);
            //ICloudflareLogHandleSercie cloudflareLogHandleSercie = new CloudflareLogHandleSercie(zoneId, authEmail, authKey, sample, startTime, endTime);
            //cloudflareLogHandleSercie.TaskStart();
            //var logs = cloudflareLogHandleSercie.GetCloudflareLogs(key);

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
            int? maxWhiteList = ActionReportBusiness.GetMaxForWhiteList("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");
            int? minWhiteList = ActionReportBusiness.GetMinForWhiteList("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");
            int? avgWhiteList = ActionReportBusiness.GetAvgForWhiteList("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");

            ActionReportBusiness.Edit(actionReport);

            int? maxAction = ActionReportBusiness.GetMaxForAction("2068c8964a4dcef78ee5103471a8db03","xx.xx.xx.xx", "comm100.com");
            int? minAction = ActionReportBusiness.GetMinForAction("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");
            int? avgAction = ActionReportBusiness.GetAvgForAction("2068c8964a4dcef78ee5103471a8db03", "xx.xx.xx.xx", "comm100.com");

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
            //SmtpQueueBusiness.Delete("06/12/2018");

            //IActiveReportService activeReportService = ActiveReportService.GetInstance();
            //activeReportService.GeneratedActiveReport();

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
