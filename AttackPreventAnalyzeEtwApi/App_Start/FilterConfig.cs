using AttackPrevent.Business;
using System;
using System.Web;
using System.Web.Mvc;

namespace AttackPreventAnalyzeEtwApi
{
    public class MyExceptionAttribute : HandleErrorAttribute
    {
        private ILogService logger = new LogService();
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            Exception ex = filterContext.Exception;
            logger.Error(ex.StackTrace);
            filterContext.ExceptionHandled = true;
        }
    }
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            filters.Add(new MyExceptionAttribute());
        }
    }
}
