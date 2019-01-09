using AttackPrevent.Model.Cloudflare;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class AnalyzeResult
    {
        public string ZoneId { get; set; }
        public List<Result> result { get; set; }
        public int timeStage { get; set; }
    }
    public class Result
    {
        public int RuleId { get; set; }
        public string Url { get; set; }
        public int Threshold { get; set; }
        public int Period { get; set; }
        public double EnlargementFactor { get; set; }
        public List<BrokenIp> BrokenIpList { get; set; }
    }
    public class BrokenIp
    {
        public string IP { get; set; }
        public List<RequestRecord> RequestRecords { get; set; }
    }
    public class RequestRecord
    {
        public string FullUrl { get; set; }
        public int RequestCount { get; set; }
        public string HostName { get; set; }
    }

    public class EtwData
    {
        public string guid { get; set; }
        public ConcurrentBag<byte[]> buffList { get; set; }
        public EnumEtwStatus enumEtwStatus { get; set; }
        public long time { get; set; }
        public int retryCount { get; set; }
        public string senderIp { get; set; }
        public List<CloudflareLog> parsedData { get; set; }

        public override bool Equals(object obj)
        {
            bool bResult = false;
            if(obj == null)
            {
                bResult = false;
            }
            else if(obj.GetType() != GetType())
            {
                bResult = false;
            }
            else
            {
                EtwData etw = obj as EtwData;
                bResult = etw.guid == this.guid;
            }
            return bResult;
        }
        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }
    }
    public enum EnumEtwStatus
    {
        None,
        Processing,
        Processed,
        Failed
    }
}
