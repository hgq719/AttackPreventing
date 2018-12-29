using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using AttackPrevent.Access;

namespace AttackPrevent.Business
{
    public class IISLogBusiness
    {
        public static readonly string cacheKey = "auditLogCache";
        
        public static dynamic GetAuditLogByPage(int limit, int offset, int zoneTableID, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            var total = IISLogAccess.GetCountByPage(zoneTableID, startTime, endTime, logType, detail);
            var rows = IISLogAccess.GetListByPage(offset, limit, zoneTableID, startTime, endTime, logType, detail);
            return new { total, rows };
        }

        public static MemoryStream ExportAuditLog(int zoneTableId, DateTime? startTime, DateTime? endTime, string logType, string detail)
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

            List<AuditLogEntity> list = IISLogAccess.GetList(zoneTableId, startTime, endTime, logType, detail);
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
            IISLogAccess.Add(item);
        }

        public static void AddList(List<AuditLogEntity> logs)
        {
            if (null != logs && logs.Count > 0)
            {
                IISLogAccess.Add(logs);
            }
        }
    }
}

