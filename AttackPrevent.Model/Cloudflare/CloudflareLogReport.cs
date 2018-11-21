using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model.Cloudflare
{
    /// <summary>
    /// 分析报表
    /// </summary>
    public class CloudflareLogReport
    {
        /// <summary>
        /// 统计GUID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 数据查询区间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 查询开始时间
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// 查询截止时间
        /// </summary>
        public DateTime End { get; set; }
        /// <summary>
        /// 数据量
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 明细
        /// </summary>
        public CloudflareLogReportItem[] CloudflareLogReportItems { get; set; }
    }
    public class CloudflareLogReportItem
    {
        public string ClientIP { get; set; }
        public string ClientRequestHost { get; set; }
        //public string ClientRequestMethod { get; set; }
        public string ClientRequestURI { get; set; }
        public int Count { get; set; }
        public bool Ban { get; set; }
    }
}
