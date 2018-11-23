﻿using AttackPrevent.Business;
using AttackPrevent.Business.Cloundflare;
using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
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
            //string authEmail = "elei.xu@comm100.com";
            //string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            //string zoneID = "2068c8964a4dcef78ee5103471a8db03";
            //DateTime start = new DateTime(2018, 11, 21, 10, 40, 43);
            //DateTime end = new DateTime(2018, 11, 21, 10, 40, 44);
            //bool retry = false;

            //ICloundFlareApiService cloundFlareApiService = new CloundFlareApiService();
            //var xx = cloundFlareApiService.GetCloudflareLogs(zoneID, authEmail, authKey, 0.01, start, end, out retry);
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
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        public ActionResult BlackList()
        {
            ViewBag.IsAdmin = IsAdmin;
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
            //return new HttpUnauthorizedResult();
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
            List<CloudflareLog> list = new List<CloudflareLog>();
  
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
            string guid = backgroundTaskService.Enqueue(zoneID, authEmail, authKey, sample, startTime, endTime);

            EnumBackgroundStatus status = backgroundTaskService.GetOperateStatus(guid);
            list = backgroundTaskService.GetCloudflareLogs(guid, limit, offset, host, siteId,url,cacheStatus,ip,responseStatus);
            var total = backgroundTaskService.GetTotal(guid, host, siteId, url, cacheStatus, ip, responseStatus);
            var rows = list;
            return Json(new { status = status.ToString(), total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetOperateStatus(string zoneID, DateTime startTime, DateTime endTime, double sample)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
            string guid = backgroundTaskService.Enqueue(zoneID, authEmail, authKey, sample, startTime, endTime);
            EnumBackgroundStatus status = backgroundTaskService.GetOperateStatus(guid);            
            return Json(new { status = status.ToString()}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportCloundflareLogs(string zoneID, DateTime startTime, DateTime endTime, string host, double sample, string siteId, string url, string cacheStatus, string ip, string responseStatus)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
            string guid = backgroundTaskService.Enqueue(zoneID, authEmail, authKey, sample, startTime, endTime);
            EnumBackgroundStatus status = backgroundTaskService.GetOperateStatus(guid);
            if(status == EnumBackgroundStatus.Succeeded)
            {         
                var list = backgroundTaskService.GetCloudflareLogs(guid, host, siteId, url, cacheStatus, ip, responseStatus);
                //创建Excel文件的对象
                XSSFWorkbook book = new XSSFWorkbook();
                //NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

                //给sheet1添加第一行的头部标题
                Type type = typeof(CloudflareLog);
                var fieldList = type.GetProperties().Select(a => a.Name);

                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                int index = 0;
                foreach (string field in fieldList)
                {
                    row1.CreateCell(index).SetCellValue(field);
                    index++;
                }

                //将数据逐步写入sheet1各个行
                for (int i = 0; i < list.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                    index = 0;
                    foreach (string field in fieldList)
                    {
                        var value = type.GetProperty(field).GetValue(list[i]);
                        rowtemp.CreateCell(index).SetCellValue(value?.ToString());
                        index++;
                    }

                }
                // 写入到客户端 
                NPOIMemoryStream ms = new NPOIMemoryStream();       
                book.Write(ms);
                //ms.Seek(0, SeekOrigin.Begin);

                MemoryStream st = new MemoryStream();
                using (ZipFile zip = ZipFile.Create(st))
                {
                    zip.BeginUpdate();
                    StreamDataSource d1 = new StreamDataSource(ms);
                    ms.IsClose = true;
                    //添加文件
                    zip.Add(d1, "CloundflareLogs.xlsx");
                    zip.CommitUpdate();
                    st.Seek(0, SeekOrigin.Begin);
                }
                ms.Close();
                ms.Dispose();
                return File(st, "application/zip", "CloundflareLogs.zip");          

            }
            else
            {
                return new HttpStatusCodeResult(404);
            }
           
        }
        public JsonResult GetWhiteLists(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string ip, string notes)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IWhiteListBusinees backgroundTaskService = new WhiteListBusinees();
            var result = backgroundTaskService.GetWhiteListModelList(zoneID, authEmail, authKey, limit, offset, ip, startTime, endTime, notes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveWhiteList(string zoneID, string ips, string comment, string vcode)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            bool isSuccessed = false;
            string errorMsg = "";
            if (vcode == "123")
            {
                IWhiteListBusinees backgroundTaskService = new WhiteListBusinees();
                string[] ipList = ips.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string ip in ipList)
                {
                    isSuccessed = backgroundTaskService.CreateAccessRule(zoneID, authEmail, authKey, ip, comment);
                    if (!isSuccessed)
                    {
                        break;
                    }
                }
            }
            else
            {
                errorMsg = "Verification code error.";
            }

            return Json(new { isSuccessed , errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteWhiteList(string zoneID, string ip)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IWhiteListBusinees backgroundTaskService = new WhiteListBusinees();
            var result = backgroundTaskService.DeleteAccessRule(zoneID, authEmail, authKey, ip);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBlackLists(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string ip, string notes)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IBlackListBusinees blackListBusinees = new BlackListBusinees();
            var result = blackListBusinees.GetBlackListModelList(zoneID, authEmail, authKey, limit, offset, ip, startTime, endTime, notes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveBlackList(string zoneID, string ips, string comment, string vcode)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            bool isSuccessed = false;
            string errorMsg = "";
            if (vcode == "123")
            {
                IBlackListBusinees blackListBusinees = new BlackListBusinees();
                string[] ipList = ips.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string ip in ipList)
                {
                    isSuccessed = blackListBusinees.CreateAccessRule(zoneID, authEmail, authKey, ip, comment);
                    if (!isSuccessed)
                    {
                        break;
                    }
                }
            }
            else
            {
                errorMsg = "Verification code error.";
            }

            return Json(new { isSuccessed, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteBlackList(string zoneID, string ip)
        {
            string authEmail = "elei.xu@comm100.com";
            string authKey = "1e26ac28b9837821af730e70163f0604b4c35";
            zoneID = "2068c8964a4dcef78ee5103471a8db03";

            IBlackListBusinees blackListBusinees = new BlackListBusinees();
            var result = blackListBusinees.DeleteAccessRule(zoneID, authEmail, authKey, ip);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
    //只有实现IStaticDataSource接口才能实现流操作
    class StreamDataSource : IStaticDataSource
    {
        public byte[] bytes { get; set; }
        public StreamDataSource(MemoryStream ms)
        {
            bytes = ms.GetBuffer();
        }

        public Stream GetSource()
        {
            Stream s = new MemoryStream(bytes);
            return s;
        }
    }
    class NPOIMemoryStream : MemoryStream
    {
        public bool IsClose { get; set; }
        public NPOIMemoryStream(bool close = false)
        {
            IsClose = close;
        }
        public override void Close()
        {
            if (IsClose)
            {
                base.Close();
            }
        }
    }
}