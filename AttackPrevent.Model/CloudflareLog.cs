using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class CloudflareLog
    {
        public string ClientRequestHost { get; set; }
        public string ClientIP { get; set; }
        public string ClientRequestURI { get; set; }
        /// <summary>
        /// all requests (including non-cacheable ones) go through the cache: also see CacheCacheStatus field
        /// </summary>
        public int CacheResponseStatus { get; set; }
        public string CacheCacheStatus { get; set; }
        public string ClientRequestMethod { get; set; }
        public string WAFAction { get; set; }

        public string WAFFlags { get; set; }

        public string WAFMatchedVar { get; set; }

        public string WAFProfile { get; set; }

        public string WAFRuleID { get; set; }

        public string WAFRuleMessage { get; set; }

        public long WorkerCPUTime { get; set; }
        public Guid Id { get; set; }
        public long CacheResponseBytes { get; set; }
        public bool CacheTieredFill { get; set; }

        public long ClientASN { get; set; }

        public string ClientCountry { get; set; }

        public string ClientDeviceType { get; set; }

        public string ClientIPClass { get; set; }

        public long ClientRequestBytes { get; set; }

        public string ClientRequestProtocol { get; set; }

        public string ClientRequestReferer { get; set; }

        public string ClientRequestUserAgent { get; set; }

        public string ClientSSLCipher { get; set; }

        public string ClientSSLProtocol { get; set; }

        public int ClientSrcPort { get; set; }

        public long EdgeColoID { get; set; }

        public string EdgeEndTimestamp { get; set; }

        public string EdgePathingOp { get; set; }

        public string EdgePathingSrc { get; set; }

        public string EdgePathingStatus { get; set; }

        public string EdgeRateLimitAction { get; set; }

        public long EdgeRateLimitID { get; set; }

        public string EdgeRequestHost { get; set; }

        public long EdgeResponseBytes { get; set; }

        public float EdgeResponseCompressionRatio { get; set; }

        public string EdgeResponseContentType { get; set; }

        public int EdgeResponseStatus { get; set; }

        public string EdgeServerIP { get; set; }

        public string EdgeStartTimestamp { get; set; }

        public string OriginIP { get; set; }

        public long OriginResponseBytes { get; set; }

        public string OriginResponseHTTPExpires { get; set; }

        public string OriginResponseHTTPLastModified { get; set; }

        public int OriginResponseStatus { get; set; }

        public long OriginResponseTime { get; set; }

        public string OriginSSLProtocol { get; set; }

        public string ParentRayID { get; set; }

        public string RayID { get; set; }

        public string SecurityLevel { get; set; }


        public string WorkerStatus { get; set; }

        public bool WorkerSubrequest { get; set; }

        public long WorkerSubrequestCount { get; set; }

        /// <summary>
        /// internal zone id
        /// </summary>
        public long ZoneID { get; set; }
    }
}
