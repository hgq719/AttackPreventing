using AttackPrevent.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class CloudflareBusiness
    {
        private string _zoneId = "xxx";
        private string _authEmail = "xx@xx.com";
        private string _authKey = "xxxyyy";

        public CloudflareBusiness(string zoneId, string authEmail, string authKey)
        {
            _zoneId = zoneId;
            _authEmail = authEmail;
            _authKey = authKey;
        }

        #region Access Rules
        public List<CloudflareAccessRule> GetAccessRules()
        {
            try
            {
                var firstPage = 1;
                var whitelist = new List<CloudflareAccessRule>();
                var requestUrl = @"https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?mode={1}&per_page=20&page={2}";
                var url = string.Format(requestUrl, _zoneId, "whitelist", firstPage);
                var content = HttpGet(url, 1200);
                var pageWhitelist = JsonConvert.DeserializeObject<CloudflareAccessRules>(content);

                if (pageWhitelist.Success)
                {
                    var resultInfo = pageWhitelist.Result_Info;
                    whitelist.AddRange(pageWhitelist.Result);
                    for (int pageIndex = 2; pageIndex <= resultInfo.Total_Pages; pageIndex++)
                    {
                        url = string.Format(requestUrl, _zoneId, "whitelist", pageIndex);
                        content = HttpGet(url, 1200);
                        pageWhitelist = JsonConvert.DeserializeObject<CloudflareAccessRules>(content);
                        if (pageWhitelist.Success)
                        {
                            whitelist.AddRange(pageWhitelist.Result);
                        }
                    }
                }
                return whitelist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> GetWhitelist(List<CloudflareAccessRule> cloudflareAccessRules)
        {
            var ipList = new List<string>();
            if (null != cloudflareAccessRules)
            {
                foreach (var rule in cloudflareAccessRules)
                {
                    var configuration = rule.Configuration;
                    if (null != configuration)
                    {
                        if ("ip".Equals(configuration.Target))
                        {
                            ipList.Add(configuration.Value);
                        }
                        else if ("ip_range".Equals(configuration.Target))
                        {
                            var ipRangeList = GetIpListFromIpRange(configuration.Value);
                            if (null != ipRangeList)
                            {
                                ipList.AddRange(ipRangeList);
                            }
                        }
                    }

                }
            }

            return ipList;
        }

        private List<string> GetIpListFromIpRange(string ipRange)
        {
            try
            {
                var ipList = new List<string>();
                if (!string.IsNullOrEmpty(ipRange))
                {
                    var ipArr = ipRange.Split('/');
                    var startIp = ipArr[0];
                    var startIpArr = ipArr[0].Split('.');
                    var endIp = string.Format("{0}.{1}.{2}.{3}", startIpArr[0], startIpArr[1], startIpArr[2], ipArr[1]);
                    for (int start = int.Parse(startIpArr[3]); start <= int.Parse(ipArr[1]); start++)
                    {
                        ipList.Add(string.Format("{0}.{1}.{2}.{3}", startIpArr[0], startIpArr[1], startIpArr[2], start));
                    }
                }
                return ipList;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region Logs

        public List<CloudflareLog> GetLogs(DateTime start, DateTime end, double sample, out bool retry)
        {
            retry = false;
            List<CloudflareLog> CloudflareLogs = new List<CloudflareLog>();
            string fields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestURI,EdgeEndTimestamp,EdgeResponseBytes,EdgeResponseStatus,EdgeStartTimestamp,CacheResponseStatus,ClientRequestBytes,CacheCacheStatus,OriginResponseStatus,OriginResponseTime";
            string startTime = GetUTCTimeString(start);
            string endTime = GetUTCTimeString(end);
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
            url = string.Format(url, _zoneId, startTime, endTime, fields, sample);
            string content = HttpGet(url, 1200);
            if (content.Contains("\"}"))
            {
                content = content.Replace("\"}", "\"},");
                CloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>(string.Format("[{0}]", content));
            }
            else
            {
                if (content.Contains("429 Too Many Requests"))
                {
                    retry = true;
                }
                //logger.Error(content);
            }
            return CloudflareLogs;
        }
        #endregion

        #region Rate Limits
        public List<RateLimitRule> GetRateLimits()
        {
            try
            {
                var requestUrl = @"https://api.cloudflare.com/client/v4/zones/{0}/rate_limits?per_page=1000";
                var url = string.Format(requestUrl, _zoneId);
                var content = HttpGet(url, 1200);
                var rules =  JsonConvert.DeserializeObject<RateLimitRules>(content);
                return rules.Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Others
        private string HttpGet(string url, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", _authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", _authKey);
                string strResult = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }

        private string GetUTCTimeString(DateTime time)
        {
            return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
        }
        #endregion
    }
}
