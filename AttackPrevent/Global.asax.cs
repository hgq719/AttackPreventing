using AttackPrevent.Business.Cloundflare;
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

            Task task = new Task(() => {
                IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
                backgroundTaskService.doWork();
            });
            task.Start();
        }
    }
}
