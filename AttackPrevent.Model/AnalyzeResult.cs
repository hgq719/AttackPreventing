using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class AnalyzeResult
    {
        public string ZoneId { get; set; }
        public Result[] result { get; set; }
    }
    public class Result
    {
        public int RuleId { get; set; }
        public string Url { get; set; }
        public int Threshold { get; set; }
        public int Period { get; set; }
        public double EnlargementFactor { get; set; }
        public BrokenIp[] BrokenIpList { get; set; }
    }
    public class BrokenIp
    {
        public string IP { get; set; }
        public RequestRecord[] RequestRecords { get; set; }
    }
    public class RequestRecord
    {
        public string FullUrl { get; set; }
        public int RequestCount { get; set; }
    }
}
