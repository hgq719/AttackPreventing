using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class HomeController : Controller
    {
        //
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult CloundflareDownloadLogs()
        {
            return View();
        }

        public ActionResult WhiteList()
        {
            return View();
        }

        public ActionResult BlackList()
        {
            return View();
        }

        public ActionResult RateLimitingList()
        {
            return View();
        }

        public ActionResult AuditLogs() 
        {
            return View();
        }

        public ActionResult AddWhiteList()
        {
            return View();
        }

        public ActionResult AddBlackList()
        {
            return View();
        }

        public ActionResult AddAndEditRateLimiting()
        {
            return View();
        }

        public ActionResult ZoneList()
        {
            return View();
        }

        public ActionResult AddZone()
        {
            return View();
        }

        public JsonResult GetAuditLog(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string logType, string detail)
        {
            List<AuditLogEntity> list = new List<AuditLogEntity>();
            for (int i = 0; i < 50; i++)
            {
                AuditLogEntity en = new AuditLogEntity();
                en.ID = i;
                en.LogType = "App";
                en.Detail = "detail" + i;
                en.LogTime = DateTime.Now;
                en.LogOperator = "Michael.he";

                list.Add(en);
            }

            var total = list.Count;
            var rows = list.Skip(offset).Take(limit).ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet); 
        }
    }
}