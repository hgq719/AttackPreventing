using AttackPrevent.Business;
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
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Controllers
{
    public class HomeController : BaseController
    {
        //
        public ActionResult Index()
        {
            //string authEmail = "";
            //string authKey = "";
            //string zoneID = "";
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
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            return View();
        }

        public ActionResult WhiteList()
        {
            ViewBag.IsAdmin = IsAdmin;
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            return View();
        }

        public ActionResult BlackList()
        {
            ViewBag.IsAdmin = IsAdmin;
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            return View();
        }

        public ActionResult AddWhiteList(string zoneId, string ip)
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            var model = new AttackPrevent.Models.WhiteListModel();
            if (!string.IsNullOrEmpty(zoneId))
            {
                model.ZoneId = zoneId;
            }
            if (!string.IsNullOrEmpty(ip))
            {
                model.IP = ip;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult AddWhiteList(Models.WhiteListModel whiteListModel)
        {
            if (ModelState.IsValid)
            {
                IWhiteListBusinees whiteListBusinees = new WhiteListBusinees();
                string[] ipList = whiteListModel.IP.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var zoneList = ZoneBusiness.GetZoneList();
                string zoneID = whiteListModel.ZoneId;
                var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
                string authEmail = zone.AuthEmail;
                string authKey = zone.AuthKey;

                foreach (string ip in ipList)
                {
                    bool isSuccessed = whiteListBusinees.CreateAccessRule(zoneID, authEmail, authKey, ip, whiteListModel.Comment);
                    if (!isSuccessed)
                    {
                        break;
                    }
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = ip,
                        LogType = LogLevel.Audit,
                        ZoneID = zoneID,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        Detail = string.Format("[Audit] {1} [{0}] Add White List successfully.", ip, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")),
                    });
                }

                return RedirectToAction("WhiteList");
            }
            else
            {
                ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
                return View(whiteListModel);
            }
        }

        public ActionResult AddBlackList()
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddBlackList(Models.BlackListModel blackListModel)
        {
            if (ModelState.IsValid)
            {
                IBlackListBusinees blackListBusinees = new BlackListBusinees();
                string[] ipList = blackListModel.IP.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var zoneList = ZoneBusiness.GetZoneList();
                string zoneID = blackListModel.ZoneId;
                var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
                string authEmail = zone.AuthEmail;
                string authKey = zone.AuthKey;

                foreach (string ip in ipList)
                {
                    bool isSuccessed = blackListBusinees.CreateAccessRule(zoneID, authEmail, authKey, ip, blackListModel.Comment);
                    if (!isSuccessed)
                    {
                        break;
                    }
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = ip,
                        LogType = LogLevel.Audit,
                        ZoneID = zoneID,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        //Detail = JsonConvert.SerializeObject(new { comment, remark = "Add BlackList", isSuccessed }),
                        Detail = string.Format("[Audit] {1} [{0}] Add Black List successfully.", ip, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")),
                    });
                }

                return RedirectToAction("BlackList");
            }
            else
            {
                ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
                return View(blackListModel);
            }
        }

        

        //Code review By Michael, 如果出错会怎么样? 我看View 里面也没有做处理.
        public JsonResult GetCloundflareLogs(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string host, double sample,string siteId,string url,string cacheStatus,string ip,string responseStatus)
        {
            var logs = new List<CloudflareLog>();
  
            var authEmail = "";
            var authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            if (zone != null)
            {
                authEmail = zone.AuthEmail;
                authKey = zone.AuthKey;
            }

            var backgroundTaskService = BackgroundTaskService.GetInstance();
            var guid = backgroundTaskService.Enqueue(zoneID, authEmail, authKey, sample, startTime, endTime);

            EnumBackgroundStatus status = backgroundTaskService.GetOperateStatus(guid).Status;
            string errorMessage = backgroundTaskService.GetOperateStatus(guid).ErrorMessage;
            logs = backgroundTaskService.GetCloudflareLogs(guid, limit, offset, host, siteId,url,cacheStatus,ip,responseStatus);
            var total = backgroundTaskService.GetTotal(guid, host, siteId, url, cacheStatus, ip, responseStatus);
            var rows = logs;
            return Json(new { status = status.ToString(), total = total, rows = rows, errorMsg = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOperateStatus(string zoneID, DateTime startTime, DateTime endTime, double sample)
        {
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            authEmail = zone.AuthEmail;
            authKey = zone.AuthKey;

            IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
            string guid = backgroundTaskService.Enqueue(zoneID, authEmail, authKey, sample, startTime, endTime);
            EnumBackgroundStatus status = backgroundTaskService.GetOperateStatus(guid).Status;
            string errorMessage = backgroundTaskService.GetOperateStatus(guid).ErrorMessage;
            return Json(new { status = status.ToString(), errorMsg = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportCloundflareLogs(string zoneID, DateTime startTime, DateTime endTime, string host, double sample, string siteId, string url, string cacheStatus, string ip, string responseStatus)
        {
            //Code Review by michael, 现在不需要给一个初始的值了，直接从数据库里面取了.
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            
            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            if (zone != null)
            {
                authEmail = zone.AuthEmail;
                authKey = zone.AuthKey;
            }

            var backgroundTaskService = BackgroundTaskService.GetInstance();
            var guid = backgroundTaskService.Enqueue(zoneID, authEmail, authKey, sample, startTime, endTime);
            var status = backgroundTaskService.GetOperateStatus(guid)?.Status;
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
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            if (zone != null)
            {
                authEmail = zone.AuthEmail;
                authKey = zone.AuthKey;
            }

            IWhiteListBusinees backgroundTaskService = new WhiteListBusinees();
            var result = backgroundTaskService.GetWhiteListModelList(zoneID, authEmail, authKey, limit, offset, ip, startTime, endTime, notes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveWhiteList(string zoneID, string ips, string comment, string vcode)
        {
            //Code review by michael, 怎么这里都还是手动写的.
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            authEmail = zone.AuthEmail;
            authKey = zone.AuthKey;

            bool isSuccessed = false;
            string errorMsg = "";

            var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
            if (vcode == configuration?.ValidateCode)
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

                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = ip,
                        LogType = LogLevel.Audit,
                        ZoneID = zoneID,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        //Detail = JsonConvert.SerializeObject(new { comment,remark= "Add WhiteList", isSuccessed }),
                        Detail = string.Format("[Audit] {1} [{0}] Add White List successfully.", ip, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")),
                    });
                }
            }
            else
            {
                errorMsg = "Verification code error.";
            }

            return Json(new { isSuccessed , errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteWhiteList(string zoneID, string ip, string vcode)
        {
            bool isSuccessed = true;
            string errorMsg = "";

            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
            if (vcode != configuration?.ValidateCode)
            {
                isSuccessed = false;
                errorMsg = "Verification code error.";
            }
            else
            {
                var zoneList = ZoneBusiness.GetZoneList();
                var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
                authEmail = zone.AuthEmail;
                authKey = zone.AuthKey;

                IWhiteListBusinees backgroundTaskService = new WhiteListBusinees();
                isSuccessed = backgroundTaskService.DeleteAccessRule(zoneID, authEmail, authKey, ip);
                if (!isSuccessed)
                {
                    errorMsg = "Delete failed.";
                }
                AuditLogBusiness.Add(new AuditLogEntity
                {
                    IP = ip,
                    LogType = LogLevel.Audit,
                    ZoneID = zoneID,
                    LogOperator = UserName,
                    LogTime = DateTime.UtcNow,
                    //Detail = JsonConvert.SerializeObject(new { remark = "Delete WhiteList", isSuccessed= result }),
                    Detail = string.Format("[Audit] {1} [{0}] Delete White List successfully.", ip, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")),
                });

            }

            return Json(new { isSuccessed, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WhiteListDetail(string ip)
        {
            ViewBag.Ip = ip;
            if (!IsAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }

        public JsonResult GetWhiteListDetail(int limit, int offset, DateTime startTime, DateTime endTime, string ip)
        {
            var backgroundTaskService = new WhiteListBusinees();
            var result = backgroundTaskService.GetWhiteListDetail(limit, offset, startTime, endTime, ip);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBlackLists(int limit, int offset, string zoneID, DateTime startTime, DateTime endTime, string ip, string notes)
        {
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            authEmail = zone.AuthEmail;
            authKey = zone.AuthKey;

            IBlackListBusinees blackListBusinees = new BlackListBusinees();
            var result = blackListBusinees.GetBlackListModelList(zoneID, authEmail, authKey, limit, offset, ip, startTime, endTime, notes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveBlackList(string zoneID, string ips, string comment, string vcode)
        {
            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            authEmail = zone.AuthEmail;
            authKey = zone.AuthKey;

            bool isSuccessed = false;
            string errorMsg = "";

            var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
            if (vcode == configuration?.ValidateCode)
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
                    AuditLogBusiness.Add(new AuditLogEntity
                    {
                        IP = ip,
                        LogType = LogLevel.Audit,
                        ZoneID = zoneID,
                        LogOperator = UserName,
                        LogTime = DateTime.UtcNow,
                        //Detail = JsonConvert.SerializeObject(new { comment, remark = "Add BlackList", isSuccessed }),
                        Detail = string.Format("[Audit] {1} [{0}] Add Black List successfully.", ip, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")),
                    });
                }
            }
            else
            {
                errorMsg = "Verification code error.";
            }

            return Json(new { isSuccessed, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteBlackList(string zoneID, string ip, string vcode)
        {
            bool isSuccessed = true;
            string errorMsg = "";

            string authEmail = "";
            string authKey = "";
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneID);
            authEmail = zone.AuthEmail;
            authKey = zone.AuthKey;


            var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
            if (vcode != configuration?.ValidateCode)
            {
                isSuccessed = false;
                errorMsg = "Verification code error.";
            }
            else
            {
                IBlackListBusinees blackListBusinees = new BlackListBusinees();
                isSuccessed = blackListBusinees.DeleteAccessRule(zoneID, authEmail, authKey, ip);
                if (!isSuccessed)
                {
                    errorMsg = "Delete failed.";
                }
                AuditLogBusiness.Add(new AuditLogEntity
                {
                    IP = ip,
                    LogType = LogLevel.Audit,
                    ZoneID = zoneID,
                    LogOperator = UserName,
                    LogTime = DateTime.UtcNow,
                    //Detail = JsonConvert.SerializeObject(new { remark = "Delete BlackList", isSuccessed = result }),
                    Detail = string.Format("[Audit] {1} [{0}] Delete Black List successfully.", ip, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")),
                });
            }

            return Json(new { isSuccessed, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 校验码
        /// </summary>
        /// <param name="validateCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckValidateCode(string validateCode)
        {
            bool bResult = false;
            var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
            if (validateCode == configuration?.ValidateCode)
            {
                bResult = true;
            }
            return Json(bResult, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ActiveReport()
        {
            ViewBag.ZoneList = ZoneBusiness.GetZoneSelectList();
            return View();
        }
        public JsonResult GetActiveReport(int limit, int offset, string zoneID, DateTime? startTime, DateTime? endTime)
        {
            dynamic result = ActionReportBusiness.GetListByPage(limit, offset, zoneID, startTime, endTime);
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