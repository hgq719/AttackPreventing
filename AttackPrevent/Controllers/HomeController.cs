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
    public class HomeController : Controller
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
            return View();
        }

        public ActionResult AuditLogs() 
        {

            List<SelectListItem> zonelist = new List<SelectListItem>() {
                new SelectListItem() {Value="111",Text="ent.comm100.com"},
                new SelectListItem() {Value="222",Text="hosted.comm100.com"},
                new SelectListItem() {Value="333",Text="app.comm100.com"},
            };

            ViewBag.ZoneList = zonelist;
            
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
    }
}