using AttackPrevent.Model.Cloudflare;
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
        private string _apiUrlPrefix = "";

        private const string CONST_WHITELIST = "whitelist";
        private const string CONST_CHALLENGE = "challenge";

        public CloudflareBusiness(string zoneId, string authEmail, string authKey, string apiUrlPrefix = @"https://api.cloudflare.com/client/v4")
        {
            _zoneId = zoneId;
            _authEmail = authEmail;
            _authKey = authKey;
            _apiUrlPrefix = apiUrlPrefix;
        }

        #region Access Rules
        public List<CloudflareAccessRule> GetBlacklist(string ip = null)
        {
            try
            {
                var firstPage = 1;
                var blacklist = new List<CloudflareAccessRule>();
                var requestUrl = string.Empty;
                var url = string.Empty;
                if (string.IsNullOrEmpty(ip))
                {
                    requestUrl = @"{3}/zones/{0}/firewall/access_rules/rules?mode={1}&per_page=20&page={2}";
                    url = string.Format(requestUrl, _zoneId, CONST_CHALLENGE, firstPage, _apiUrlPrefix);
                }
                else
                {
                    requestUrl = @"{3}/zones/{0}/firewall/access_rules/rules?mode={1}&configuration.value={2}";
                    url = string.Format(requestUrl, _zoneId, CONST_CHALLENGE, ip, _apiUrlPrefix);
                }
                
                var content = HttpGet(url, 1200);
                var pageBlacklist = JsonConvert.DeserializeObject<CloudflareAccessRuleListResponse>(content);

                if (pageBlacklist.Success)
                {
                    var resultInfo = pageBlacklist.Result_Info;
                    blacklist.AddRange(pageBlacklist.Result);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        for (int pageIndex = 2; pageIndex <= resultInfo.Total_Pages; pageIndex++)
                        {
                            url = string.Format(requestUrl, _zoneId, CONST_CHALLENGE, pageIndex, _apiUrlPrefix);
                            content = HttpGet(url, 1200);
                            pageBlacklist = JsonConvert.DeserializeObject<CloudflareAccessRuleListResponse>(content);
                            if (pageBlacklist.Success)
                            {
                                blacklist.AddRange(pageBlacklist.Result);
                            }
                        }
                    }
                }
                return blacklist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<CloudflareAccessRule> GetWhiteList(string ip = null)
        {
            try
            {
                var firstPage = 1;
                var whitelist = new List<CloudflareAccessRule>();

                var requestUrl = string.Empty;
                var url = string.Empty;
                if (string.IsNullOrEmpty(ip))
                {
                    requestUrl = @"{3}/zones/{0}/firewall/access_rules/rules?mode={1}&per_page=20&page={2}";
                    url = string.Format(requestUrl, _zoneId, CONST_WHITELIST, firstPage, _apiUrlPrefix);
                }
                else
                {
                    requestUrl = @"{3}/zones/{0}/firewall/access_rules/rules?mode={1}&configuration.value={2}";
                    url = string.Format(requestUrl, _zoneId, CONST_WHITELIST, ip, _apiUrlPrefix);
                }
                var content = HttpGet(url, 1200);
                var pageWhitelist = JsonConvert.DeserializeObject<CloudflareAccessRuleListResponse>(content);

                if (pageWhitelist.Success)
                {
                    var resultInfo = pageWhitelist.Result_Info;
                    whitelist.AddRange(pageWhitelist.Result);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        for (int pageIndex = 2; pageIndex <= resultInfo.Total_Pages; pageIndex++)
                        {
                            url = string.Format(requestUrl, _zoneId, CONST_WHITELIST, pageIndex, _apiUrlPrefix);
                            content = HttpGet(url, 1200);
                            pageWhitelist = JsonConvert.DeserializeObject<CloudflareAccessRuleListResponse>(content);
                            if (pageWhitelist.Success)
                            {
                                whitelist.AddRange(pageWhitelist.Result);
                            }
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

        public List<string> GetIpWhitelist()
        {
            var cloudflareAccessRules = GetWhiteList();
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

        public CloudflareAccessRuleResponse CreateAccessRule(CloudflareAccessRuleRequest request)
        {
            string url = "{0}/zones/{1}/firewall/access_rules/rules";
            url = string.Format(url,_apiUrlPrefix, _zoneId);
            string json = JsonConvert.SerializeObject(request);
            string content = HttpPost(url, json, 90);
            var response = JsonConvert.DeserializeObject<CloudflareAccessRuleResponse>(content);
            return response;
        }

        public CloudflareAccessRuleResponse DeleteAccessRule(string id)
        {
            string url = "{2}/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, _zoneId, id, _apiUrlPrefix);
            string json = JsonConvert.SerializeObject(new { cascade = "none" });
            string content = HttpDelete(url, json, 90);
            return JsonConvert.DeserializeObject<CloudflareAccessRuleResponse>(content);
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
            string url = "https://{5}/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
            url = string.Format(url, _zoneId, startTime, endTime, fields, sample, _apiUrlPrefix);
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
        public List<CloudflareRateLimitRule> GetRateLimits()
        {
            try
            {
                var requestUrl = @"{1}/zones/{0}/rate_limits?per_page=1000";
                var url = string.Format(requestUrl, _zoneId,_apiUrlPrefix);
                var content = HttpGet(url, 1200);
                var rules =  JsonConvert.DeserializeObject<RateLimitRuleResponse>(content);
                return rules.Result;
            }
            catch (Exception )
            {
                return null;
            }
        }

        public CreateRateLimitResponse CreateRateLimit(CloudflareRateLimitRule rateLimitRule)
        {
            CreateRateLimitResponse createRateLimitResponse = new CreateRateLimitResponse();
            string url = "{1}/zones/{0}/rate_limits";
            url = string.Format(url, _zoneId, _apiUrlPrefix);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPost(url, json, 90);
            createRateLimitResponse = JsonConvert.DeserializeObject<CreateRateLimitResponse>(content);
            return createRateLimitResponse;
        }
        public UpdateRateLimitResponse UpdateRateLimit(CloudflareRateLimitRule rateLimitRule)
        {
            UpdateRateLimitResponse updateRateLimitResponse = new UpdateRateLimitResponse();
            string url = "{1}/zones/{0}/rate_limits";
            url = string.Format(url, _zoneId,_apiUrlPrefix);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPut(url, json, 90);
            updateRateLimitResponse = JsonConvert.DeserializeObject<UpdateRateLimitResponse>(content);
            return updateRateLimitResponse;
        }
        public DeleteRateLimitResponse DeleteRateLimit(string id)
        {
            DeleteRateLimitResponse deleteRateLimitResponse = new DeleteRateLimitResponse();
            string url = "{2}/zones/{0}/rate_limits/{1}";
            url = string.Format(url, _zoneId, id, _apiUrlPrefix);
            string json = "";
            string content = HttpDelete(url, json, 90);
            deleteRateLimitResponse = JsonConvert.DeserializeObject<DeleteRateLimitResponse>(content);
            return deleteRateLimitResponse;
        }

        public CloudflareRateLimitRule GetRateLimitRule(string url)
        {
            var ratelimits = GetRateLimits();
            if (null != ratelimits && ratelimits.Count > 0)
            {
                var id = string.Empty;
                foreach (var rateLimit in ratelimits)
                {
                    if (url.Contains(rateLimit.Match.Request.Url))
                    {
                        return rateLimit;
                    }
                }
            }
            return null;
        }

        public bool OpenRateLimit(string url, int threshold, int period)
        {
            var ratelimit = GetRateLimitRule(url);
            if (null != ratelimit&& ratelimit.Threshold == threshold && ratelimit.Period == period)
            {
                ratelimit.Disabled = false;
                var response = UpdateRateLimit(ratelimit);
                return response.success;
            }
            else
            {
                var response = CreateRateLimit(new CloudflareRateLimitRule(url, threshold, period));
                return response.success;
            }
        }

        public bool RemoveRateLimit(string url, int threshold, int period)
        {
            var ratelimit = GetRateLimitRule(url);
            if (null != ratelimit && ratelimit.Threshold == threshold && ratelimit.Period == period)
            {
                var response = DeleteRateLimit(ratelimit.Id);
                return response.success;
            }
            return true;
        }
        #endregion

        #region Ban IP
        public CloudflareAccessRuleResponse BanIp(string ip, string notes)
        {
            var blackListRequest = new CloudflareAccessRuleRequest(ip,"challenge", false, notes);
            return CreateAccessRule(blackListRequest);
        }

        public CloudflareAccessRuleResponse DeleteBanIp(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
            {
                var blacklist = GetBlacklist(ip);
                if (null != blacklist && blacklist.Count > 0)
                {
                    var blackInfo = blacklist[0];
                    return DeleteAccessRule(blackInfo.Id);
                }
            }
            return new CloudflareAccessRuleResponse()
            {
                Success = false
            };
        }
        #endregion

        #region WhiteList
        public CloudflareAccessRuleResponse AddWhiteList(string ip, bool ifIpRange, string notes)
        {
            var blackListRequest = new CloudflareAccessRuleRequest(ip, CONST_WHITELIST, ifIpRange, notes);
            return CreateAccessRule(blackListRequest);
        }

        public CloudflareAccessRuleResponse DeleteIpWhitelist(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
            {
                var whitelist = GetWhiteList(ip);
                if (null != whitelist && whitelist.Count > 0)
                {
                    var whiteInfo = whitelist[0];
                    return DeleteAccessRule(whiteInfo.Id);
                }
            }
            return new CloudflareAccessRuleResponse()
            {
                Success = false
            };
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

        private string HttpPost(string url, string json, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", _authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", _authKey);
                string strResult = client.PostAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }

        private string HttpPut(string url, string json, int timeout = 90)
        {
            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", _authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", _authKey);
                string strResult = client.PutAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }

        private string HttpDelete(string url, string json, int timeout = 90)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            if (!string.IsNullOrWhiteSpace(json))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", _authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", _authKey);
                string strResult = client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
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
