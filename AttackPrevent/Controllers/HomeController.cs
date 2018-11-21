using AttackPrevent.Business;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class HomeController : BaseController
    {
        //
        public ActionResult Index()
        {
            //Business.ILogService logService = new Business.LogService();
            //logService.Debug("test log4net");
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
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            return View();
        }

        public JsonResult GetRateLimiting(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string url)
        {
            dynamic result = RateLimitBusiness.GetAuditLog(limit, offset, zoneID, startTime, endTime, url);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddAndEditRateLimiting()
        {
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }

        public ActionResult AuditLogs() 
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            
            return View();
        }

        public ActionResult AddWhiteList()
        {
            return View();
        }

        public ActionResult AddBlackList()
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            return View();
        }        

        public ActionResult ZoneList()
        {
            return new HttpUnauthorizedResult();
            return View();
        }

        //private void ck()
        //{
        //    throw new HttpUnauthorizedResult();
        //}

        public ActionResult AddZone()
        {
            return View();
        }

        public JsonResult GetAuditLog(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string logType, string detail)
        {   
            dynamic result = AuditLogBusiness.GetAuditLog(limit, offset, zoneID, startTime, endTime, logType, detail);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public FileResult ExportAuditLog(string zoneID, DateTime startTime, DateTime endTime, string logType, string detail) 
        {
            MemoryStream ms = AuditLogBusiness.ExportAuditLog(zoneID, startTime, endTime, logType, detail);
            return File(ms, "application/vnd.ms-excel", "AuditLog.xls");
        }
        public JsonResult GetCloundflareLogs(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string host, double sample,string siteId,string url,string cacheStatus,string ip,string responseStatus)
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