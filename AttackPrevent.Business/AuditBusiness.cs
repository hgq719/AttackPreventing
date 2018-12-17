using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using AttackPrevent.Access;

namespace AttackPrevent.Business
{
    public class AuditLogBusiness
    {
        public static readonly string cacheKey = "auditLogCache";

        public static dynamic GetAuditLog(int limit, int offset, int zoneTableID, DateTime? startTime, DateTime? endTime, string logType, string detail, bool ifUseCache, string userName)
        {
            //List<AuditLogEntity> list = new List<AuditLogEntity>();
            //for (int i = 0; i < 50; i++)
            //{
            //    AuditLogEntity en = new AuditLogEntity();
            //    en.ID = i;
            //    en.LogType = "App";
            //    en.Detail = "detail" + i;
            //    en.LogTime = DateTime.Now;
            //    en.LogOperator = "Michael.he";

            //    list.Add(en);
            //}

            if (ifUseCache)
            {
                List<AuditLogEntity> list = Utils.GetMemoryCache<List<AuditLogEntity>>(cacheKey+userName);
                if (list == null)
                {
                    list = new List<AuditLogEntity>();
                }
                var total = list.Count;
                var rows = list.Skip(offset).Take(limit).ToList();

                return new { total, rows };
            }
            else
            {
                Utils.RemoveMemoryCache(cacheKey+userName);
                List<AuditLogEntity> list = AuditLogAccess.GetList(zoneTableID, startTime, endTime, logType, detail);
                Utils.SetMemoryCache(cacheKey+userName, list);
                //string cacheKeyStr = cacheKey + userName;

                return new { cacheKey = cacheKey + userName };
            }
            
        }

        public static MemoryStream ExportAuditLog(int zoneTableId, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据
            //List<AuditLogEntity> list = new List<AuditLogEntity>();
            //for (int i = 0; i < 50; i++)
            //{
            //    AuditLogEntity en = new AuditLogEntity();
            //    en.ID = i;
            //    en.LogType = "App";
            //    en.Detail = "detail" + i;
            //    en.LogTime = DateTime.Now;
            //    en.LogOperator = "Michael.he";

            //    list.Add(en);
            //}

            List<AuditLogEntity> list = AuditLogAccess.GetList(zoneTableId, startTime, endTime, logType, detail);
            //给sheet1添加第一行的头部标题
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("Log Type");
            row1.CreateCell(1).SetCellValue("Detail");
            row1.CreateCell(2).SetCellValue("Log Time");
            row1.CreateCell(3).SetCellValue("Log Operator");
            var notesStyle = book.CreateCellStyle();
            notesStyle.WrapText = true;//设置换行这个要先设置
            //将数据逐步写入sheet1各个行
            for (int i = 0; i < list.Count; i++)
            {
                var rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(list[i].LogType.ToString());
                rowtemp.CreateCell(1).SetCellValue(list[i].Detail.Replace("<br />","\n"));
                rowtemp.Cells[1].CellStyle = notesStyle;
                rowtemp.CreateCell(2).SetCellValue(list[i].LogTime.ToString(CultureInfo.InvariantCulture));
                rowtemp.CreateCell(3).SetCellValue(list[i].LogOperator);
            }

            sheet1.SetColumnWidth(1, 160 * 256);
            sheet1.SetColumnWidth(2, 20 * 256);
            // 写入到客户端 
            var ms = new MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public static void Add(AuditLogEntity item)
        {
            AuditLogAccess.Add(item);
        }

        public static void AddList(List<AuditLogEntity> logs)
        {
            if (null != logs && logs.Count > 0)
            {
                AuditLogAccess.Add(logs);
            }
        }
    }
}

