using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AttackPrevent.Business
{
    public class CookieHelper
    {
        //是否已经被创建
        public static bool IsCreate(string name)
        {
            HttpCookie Cookie = HttpContext.Current.Request.Cookies[name];
            return Cookie != null;
        }

        //设置Cookies
        public static void SetCookie(string name, string value, DateTime Expires)
        {
            HttpContext.Current.Response.Cookies[name].Value = value;
            HttpContext.Current.Response.Cookies[name].Expires = Expires;
        }

        //获取Cookie
        public static string GetCookie(string name)
        {
            return HttpContext.Current.Request.Cookies[name] == null ? string.Empty : HttpContext.Current.Request.Cookies[name].Value;
        }

        //清空Cookie
        public void ClearCookie(string name)
        {
            HttpContext.Current.Request.Cookies[name].Expires = DateTime.UtcNow.AddDays(-1);
        }
    }
}
