using AttackPrevent.Business;
using AttackPrevent.Model;
using System;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent
{
    public class MyExceptionAttribute : HandleErrorAttribute
    {
        //如果很多用户都出错，同时将这些错误写入到日志中，会造成日志文件的并发，所以将每个用户的错误存储在队列中去，队列操作是非常的迅速的
        //public static Queue<Exception> MyExceptionQueue = new Queue<Exception>();

        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            Exception ex = filterContext.Exception;
            //接下来就是得加入到队列中进行处理
            //MyExceptionQueue.Enqueue(ex);
            //跳转到错误页面
            //filterContext.HttpContext.Response.Redirect("/Error.html");
            //AuditLogBusiness.Add(new Model.AuditLogEntity()
            //{
            //    IP = filterContext.HttpContext.Request.UserHostAddress,
            //    LogType = LogLevel.Error.ToString(),
            //    ZoneID = string.Empty,
            //    LogOperator = string.Empty,
            //    LogTime = DateTime.UtcNow,
            //    Detail = $"[Audit] Message {ex.Message} " +
            //    $"StackTrace {ex.StackTrace}",
            //});
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/HomeError.cshtml",
                ViewData = new ViewDataDictionary<Exception>() {
                    {"ex", ex }
                },
            };
            filterContext.ExceptionHandled = true;
        }
    }

    public class ActionFilterAttribute : System.Web.Mvc.FilterAttribute, System.Web.Mvc.IActionFilter
    {
        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext) { }
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            string selectedMenu = "CloundflareDownloadLogs";
            switch (filterContext.ActionDescriptor.ActionName)
            {
                case "CloundflareDownloadLogs":
                    selectedMenu = "CloundflareDownloadLogs";
                    break;
                case "WhiteList":
                case "AddWhiteList":
                case "WhiteListDetail":
                    selectedMenu = "WhiteList";
                    break;
                case "BlackList":
                case "AddBlackList":
                    selectedMenu = "WhiteList";
                    break;
                case "RateLimitIndex":
                case "AddRateLimiting":
                case "EditRateLimiting":
                    selectedMenu = "RateLimit";
                    break;
                case "AuditLogs":
                    selectedMenu = "AuditLog";
                    break;
                case "IISLogs":
                    selectedMenu = "IISLog";
                    break;
                case "ZoneIndex":
                case "AddZone":
                case "EditZone":
                    selectedMenu = "Zone";
                    break;
                case "ActiveReport":
                    selectedMenu = "ActiveReport";
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(filterContext.HttpContext.Session["selectedMenu"] as string))
                    {
                        selectedMenu = filterContext.HttpContext.Session["selectedMenu"].ToString();
                    }
                    break;
            }
            filterContext.HttpContext.Session["selectedMenu"] = selectedMenu;
        }
    }
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            filters.Add(new MyExceptionAttribute());
            filters.Add(new ActionFilterAttribute());
        }
    }
}
