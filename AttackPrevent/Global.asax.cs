using AttackPrevent.Business;
using AttackPrevent.Business.Cloundflare;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AttackPrevent
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //配置log4
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(Server.MapPath("~/Web.config")));

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // 使api返回为json 
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            //MapperInitialize.Initialize();
            RunProgram().GetAwaiter();

            //Task task = new Task(() =>
            //{
            //    IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
            //    backgroundTaskService.doWork();
            //});
            //task.Start();

            //Task task2 = new Task(() =>
            //{
            //    ILogService logger = new LogService();
            //    IEtwAnalyzeService etwAnalyzeService = EtwAnalyzeService.GetInstance();
            //    etwAnalyzeService.doWork();
            //    logger.Debug("etwAnalyzeService start.");
            //});
            //task2.Start();
        }
        #region Start Quartz
        async Task RunProgram()
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.ToString());
            }
        }
        #endregion
    }
}
