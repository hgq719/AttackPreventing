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

        public FileResult ExportAuditLog(string zoneID, DateTime startTime, DateTime endTime, string logType, string detail) 
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据
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
            //给sheet1添加第一行的头部标题
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("Log Type");
            row1.CreateCell(1).SetCellValue("Detail");
            row1.CreateCell(2).SetCellValue("Log Time");
            row1.CreateCell(3).SetCellValue("Log Operator");
            //将数据逐步写入sheet1各个行
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(list[i].LogType);
                rowtemp.CreateCell(1).SetCellValue(list[i].Detail);
                rowtemp.CreateCell(2).SetCellValue(list[i].LogTime.ToString());
                rowtemp.CreateCell(3).SetCellValue(list[i].LogOperator);
            }
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
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