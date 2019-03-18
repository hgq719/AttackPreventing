using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AttackPrevent.Business
{
    public interface ICloudFlareApiService
    {
        Task<List<CloudflareLog>> GetCloudflareLogsAsync(DateTime start, DateTime end);
        List<CloudflareLog> GetCloudflareLogs(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end, out bool retry);
        List<FirewallAccessRule> GetAccessRuleList(string zoneId, string authEmail, string authKey, string ip, string notes);
        List<FirewallAccessRule> GetAccessRuleList(string zoneId, string authEmail, string authKey, EnumMode mode);
        /// <summary>
        /// valid values: block, challenge, whitelist, js_challenge
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        FirewallAccessRuleResponse CreateAccessRule(string zoneId, string authEmail, string authKey, FirewallAccessRuleRequest request);
        FirewallAccessRuleResponse EditAccessRule(string zoneId, string authEmail, string authKey, string id, FirewallAccessRuleRequest request);
        FirewallAccessRuleResponse DeleteAccessRule(string zoneId, string authEmail, string authKey, string id);
        List<RateLimitRule> GetRateLimitRuleList(string zoneId, string authEmail, string authKey);
        CreateRateLimitResponse CreateRateLimit(string zoneId, string authEmail, string authKey, RateLimitRule rateLimitRule);
        UpdateRateLimitResponse UpdateRateLimit(string zoneId, string authEmail, string authKey, RateLimitRule rateLimitRule);
        DeleteRateLimitResponse DeleteRateLimit(string zoneId, string authEmail, string authKey, string id);
        string CheckAuth(string zoneId, string authEmail, string authKey);
    }
    public class CloudFlareApiService : ICloudFlareApiService
    {
        private string _zoneId = "xxx";
        private string _authEmail = "xx@xx.com";
        private string _authKey = "xxxyyy";
        private string _apiUrlPrefix = "";

        private const string CONST_WHITELIST = "whitelist";
        private const string CONST_CHALLENGE = "challenge";

        private ILogService logger = new LogService();

        public CloudFlareApiService()
        {
        }

        public CloudFlareApiService(string zoneId, string authEmail, string authKey, string apiUrlPrefix = @"https://api.cloudflare.com/client/v4")
        {
            _zoneId = zoneId;
            _authEmail = authEmail;
            _authKey = authKey;
            _apiUrlPrefix = apiUrlPrefix;
        }

        public async Task<List<CloudflareLog>> GetCloudflareLogsAsync(DateTime start, DateTime end)
        {
            List<CloudflareLog> CloudflareLogs = new List<CloudflareLog>();
            //string fields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestURI,EdgeEndTimestamp,EdgeResponseBytes,EdgeResponseStatus,EdgeStartTimestamp,CacheResponseStatus,ClientRequestBytes,CacheCacheStatus,OriginResponseStatus,OriginResponseTime";
            //string startTime = GetUTCTimeString(start);
            //string endTime = GetUTCTimeString(end);
            //string url = "https://api.cloudflare.com/client/v4/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
            //url = string.Format(url, zoneId, startTime, endTime, fields, sample);
            //string content = await HttpGetAsyc(url, 1200);
            //if (content.Contains("\"}"))
            //{
            //    content = content.Replace("\"}", "\"},");
            //    CloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>(string.Format("[{0}]", content));
            //}
            return CloudflareLogs;
        }
        public List<CloudflareLog> GetCloudflareLogs(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end, out bool retry)
        {
            retry = false;
            List<CloudflareLog> CloudflareLogs = new List<CloudflareLog>();
            string fields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestURI,EdgeEndTimestamp,EdgeResponseBytes,EdgeResponseStatus,EdgeStartTimestamp,CacheResponseStatus,ClientRequestBytes,CacheCacheStatus,OriginResponseStatus,OriginResponseTime";
            string startTime = GetUTCTimeString(start);
            string endTime = GetUTCTimeString(end);
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
            url = string.Format(url, zoneId, startTime, endTime, fields, sample);
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpGet(authEmail, authKeyDecrypt, url, 1200);
            if (content.Contains("\"}") && !content.Contains("{\"success\":false"))
            {
                content = content.Replace("\"}", "\"},");
                CloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>(string.Format("[{0}]", content));
            }
            else
            {
                if(content.Contains("429 Too Many Requests") ||
                    content.StartsWith("Rate limited.") )
                {
                    retry = true;
                }

                if( !string.IsNullOrEmpty(content))
                {
                    logger.Error(content);
                }                    
            }
            return CloudflareLogs;
        }        
        public List<FirewallAccessRule> GetAccessRuleList(string zoneId, string authEmail, string authKey, string ip, string notes)
        {
            List<FirewallAccessRule> firewallAccessRules = new List<FirewallAccessRule>();
            int page = 1;
            while (true)
            {
                //?page={1}&per_page={2}&notes=my note
                string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?page={1}&per_page=500&configuration.value={2}";
                url = string.Format(url, zoneId, page, ip);
                if (!string.IsNullOrEmpty(notes))
                {
                    url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?page={1}&per_page=500&notes={2}";
                    url = string.Format(url, zoneId, page, notes);
                }
                //解密
                var authKeyDecrypt = Utils.AesDecrypt(authKey);
                string content = HttpGet(authEmail, authKeyDecrypt, url, 1200);
                FirewallAccessRuleResponseList firewallAccessRuleResponseList = JsonConvert.DeserializeObject<FirewallAccessRuleResponseList>(content);
                if (firewallAccessRuleResponseList.success)
                {
                    foreach (CreateResult result in firewallAccessRuleResponseList.result)
                    {
                        firewallAccessRules.Add(new FirewallAccessRule
                        {
                            id = result.id,
                            notes = result.notes,
                            mode = result.mode,
                            configurationTarget = result.configuration.target,
                            configurationValue = result.configuration.value,
                            createTime = result.created_on,
                            modifiedTime = result.modified_on,
                            scopeId = result.scope.id,
                            scopeEmail = result.scope.email,
                            scopeType = result.scope.type,
                        });
                    }
                    
                    if(firewallAccessRuleResponseList.result_info.page >= firewallAccessRuleResponseList.result_info.total_pages)
                    {
                        break;
                    }
                    else
                    {
                        page++;
                    }
                }
                else
                {
                    break;
                }
            }            
            return firewallAccessRules;
        }
        public FirewallAccessRuleResponse CreateAccessRule(string zoneId, string authEmail, string authKey, FirewallAccessRuleRequest request)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(request);
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpPost(authEmail, authKeyDecrypt, url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public FirewallAccessRuleResponse EditAccessRule(string zoneId, string authEmail, string authKey, string id, FirewallAccessRuleRequest request)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, zoneId, id);
            string json = JsonConvert.SerializeObject(request);
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpPut(authEmail, authKeyDecrypt, url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public FirewallAccessRuleResponse DeleteAccessRule(string zoneId, string authEmail, string authKey, string id)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, zoneId, id);
            string json = JsonConvert.SerializeObject(new { cascade = "none" });
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpDelete(authEmail, authKeyDecrypt, url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public List<RateLimitRule> GetRateLimitRuleList(string zoneId, string authEmail, string authKey)
        {
            List<RateLimitRule> rateLimitRules = new List<RateLimitRule>();
            int page = 1;
            while (true)
            {
                //?page={1}&per_page={2}&notes=my note
                string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits?page={1}&per_page=20";
                url = string.Format(url, zoneId, page);
                //解密
                var authKeyDecrypt = Utils.AesDecrypt(authKey);
                string content = HttpGet(authEmail, authKeyDecrypt, url, 1200);
                RateLimitResponse rateLimitResponse = JsonConvert.DeserializeObject<RateLimitResponse>(content);
                if (rateLimitResponse.success)
                {
                    rateLimitRules.AddRange(rateLimitResponse.result);
                    if (rateLimitResponse.result_info.page >= rateLimitResponse.result_info.total_pages)
                    {
                        break;
                    }
                    else
                    {
                        page++;
                    }
                }
                else
                {
                    break;
                }
            }
            return rateLimitRules;
        }
        public CreateRateLimitResponse CreateRateLimit(string zoneId, string authEmail, string authKey, RateLimitRule rateLimitRule)
        {
            CreateRateLimitResponse createRateLimitResponse = new CreateRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpPost(authEmail, authKeyDecrypt, url, json, 90);
            createRateLimitResponse = JsonConvert.DeserializeObject<CreateRateLimitResponse>(content);
            return createRateLimitResponse;
        }
        public UpdateRateLimitResponse UpdateRateLimit(string zoneId, string authEmail, string authKey, RateLimitRule rateLimitRule)
        {
            UpdateRateLimitResponse updateRateLimitResponse = new UpdateRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpPut(authEmail, authKeyDecrypt, url, json, 90);
            updateRateLimitResponse = JsonConvert.DeserializeObject<UpdateRateLimitResponse>(content);
            return updateRateLimitResponse;
        }
        public DeleteRateLimitResponse DeleteRateLimit(string zoneId, string authEmail, string authKey, string id)
        {
            DeleteRateLimitResponse deleteRateLimitResponse = new DeleteRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits/{1}";
            url = string.Format(url, zoneId, id);
            string json = "";
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpDelete(authEmail, authKeyDecrypt, url, json, 90);
            deleteRateLimitResponse = JsonConvert.DeserializeObject<DeleteRateLimitResponse>(content);
            return deleteRateLimitResponse;
        }
        public string CheckAuth(string zoneId, string authEmail, string authKey)
        {
            string errorMessage = "";
            string ip = "xxx.xxx.xxx.xxx";

            //?page={1}&per_page={2}&notes=my note
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?page=1&per_page=500&configuration.value={1}";
            url = string.Format(url, zoneId, ip);
            //解密
            var authKeyDecrypt = Utils.AesDecrypt(authKey);
            string content = HttpGet(authEmail, authKeyDecrypt, url);
            FirewallAccessRuleResponseList firewallAccessRuleResponseList = JsonConvert.DeserializeObject<FirewallAccessRuleResponseList>(content);
            if (firewallAccessRuleResponseList.success)
            {
            }
            else
            {
                if (firewallAccessRuleResponseList.errors.Any(a => a.message.Contains("Authentication error")))
                {
                    errorMessage = firewallAccessRuleResponseList.errors.FirstOrDefault()?.message;
                }
            }

            return errorMessage;
        }

        private string HttpGet(string authEmail, string authKey, string url, int timeout = 90)
        {
            //处理HttpWebRequest访问https有安全证书的问题（ 请求被中止: 未能创建 SSL/TLS 安全通道。）
            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private async Task<string> HttpGetAsyc(string authEmail, string authKey, string url, int timeout = 90)
        {
            //处理HttpWebRequest访问https有安全证书的问题（ 请求被中止: 未能创建 SSL/TLS 安全通道。）
            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                HttpResponseMessage httpResponseMessage = await client.GetAsync(url);
                string strResult = await httpResponseMessage.Content.ReadAsStringAsync();
                return strResult;
            }
        }
        private string HttpPost(string authEmail, string authKey, string url, string json, int timeout = 90)
        {
            //处理HttpWebRequest访问https有安全证书的问题（ 请求被中止: 未能创建 SSL/TLS 安全通道。）
            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.PostAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private string HttpPut(string authEmail, string authKey, string url, string json, int timeout = 90)
        {
            //处理HttpWebRequest访问https有安全证书的问题（ 请求被中止: 未能创建 SSL/TLS 安全通道。）
            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.PutAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private string HttpDelete(string authEmail, string authKey, string url, string json, int timeout = 90)
        {
            //处理HttpWebRequest访问https有安全证书的问题（ 请求被中止: 未能创建 SSL/TLS 安全通道。）
            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            if (!string.IsNullOrWhiteSpace(json))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("X-Auth-Email", authEmail);
                client.DefaultRequestHeaders.Add("X-Auth-Key", authKey);
                string strResult = client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                return strResult;
            }
        }
        private string GetUTCTimeString(DateTime time)
        {
            //return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
            return time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
        }

        public List<FirewallAccessRule> GetAccessRuleList(string zoneId, string authEmail, string authKey, EnumMode mode)
        {
            List<FirewallAccessRule> firewallAccessRules = new List<FirewallAccessRule>();
            int page = 1;
            while (true)
            {
                //?page={1}&per_page={2}&notes=my note
                string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules?page={1}&per_page=500&mode={2}";
                url = string.Format(url, zoneId, page, mode.ToString());
                //解密
                var authKeyDecrypt = Utils.AesDecrypt(authKey);
                string content = HttpGet(authEmail, authKeyDecrypt, url, 1200);
                FirewallAccessRuleResponseList firewallAccessRuleResponseList = JsonConvert.DeserializeObject<FirewallAccessRuleResponseList>(content);
                if (firewallAccessRuleResponseList.success)
                {
                    foreach (CreateResult result in firewallAccessRuleResponseList.result)
                    {
                        firewallAccessRules.Add(new FirewallAccessRule
                        {
                            id = result.id,
                            notes = System.Web.HttpUtility.HtmlDecode(result.notes),
                            mode = result.mode,
                            configurationTarget = result.configuration.target,
                            configurationValue = result.configuration.value,
                            createTime = result.created_on,
                            modifiedTime = result.modified_on,
                            scopeId = result.scope.id,
                            scopeEmail = result.scope.email,
                            scopeType = result.scope.type,
                        });
                    }

                    if (firewallAccessRuleResponseList.result_info.page >= firewallAccessRuleResponseList.result_info.total_pages)
                    {
                        break;
                    }
                    else
                    {
                        page++;
                    }
                }
                else
                {
                    break;
                }
            }
            return firewallAccessRules;
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
                    requestUrl = @"{3}/zones/{0}/firewall/access_rules/rules?mode={1}&per_page=200&page={2}";
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
                    if (string.IsNullOrEmpty(ip))
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
                    requestUrl = @"{3}/zones/{0}/firewall/access_rules/rules?mode={1}&per_page=200&page={2}";
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
                    if (string.IsNullOrEmpty(ip))
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
            url = string.Format(url, _apiUrlPrefix, _zoneId);
            string json = JsonConvert.SerializeObject(request);
            string content = HttpPost(url, json, 90);
            var response = JsonConvert.DeserializeObject<CloudflareAccessRuleResponse>(content);
            return response;
        }

        public CloudflareAccessRuleResponse DeleteAccessRule(string id)
        {
            var url = $"{_apiUrlPrefix}/zones/{_zoneId}/firewall/access_rules/rules/{id}";
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
                    if (ipArr[1] == "24")
                    {
                        ipArr[1] = "254";
                    }
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
            var zoneTableId = ZoneBusiness.GetZoneByZoneId(_zoneId).TableID;
            retry = false;
            var cloudflareLogs = new List<CloudflareLog>();
            try
            {

                string fields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestURI,EdgeEndTimestamp,EdgeResponseBytes,EdgeResponseStatus,EdgeStartTimestamp,CacheResponseStatus,ClientRequestBytes,CacheCacheStatus,OriginResponseStatus,OriginResponseTime";
                string startTime = GetUTCTimeString(start);
                string endTime = GetUTCTimeString(end);
                string url = "{5}/zones/{0}/logs/received?start={1}&end={2}&fields={3}&sample={4}";
                url = string.Format(url, _zoneId, startTime, endTime, fields, sample, _apiUrlPrefix);
                string content = HttpGet(url,240);
                if(content.Contains(@"""success"":false"))
                {
                    if (content.Contains("429 Too Many Requests"))
                    {
                        retry = true;
                    }
                    else
                    {
                        var errorResponse = JsonConvert.DeserializeObject<CloudflareLogErrorResponse>(content);
                        AuditLogBusiness.Add(new AuditLogEntity(zoneTableId, LogLevel.Error,
                            $"Got logs failure, the reason is:[{ (errorResponse.Errors.Count > 0 ? errorResponse.Errors[0].Message : "No error message from Cloudflare.")}]."));
                    }
                }
                else
                {
                    content = content.Replace("\"}", "\"},");
                    cloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>($"[{content}]");
                }

                return cloudflareLogs;
            }
            catch (Exception ex)
            {
                retry = true;
                AuditLogBusiness.Add(new AuditLogEntity(zoneTableId, LogLevel.Error,
                    $"Got logs failure, the reason is:[{ex.Message}]. <br />stack trace:{ex.StackTrace}]."));
                return cloudflareLogs;
            }
        }
        #endregion

        #region Rate Limits
        public List<CloudflareRateLimitRule> GetRateLimits()
        {
            try
            {
                var requestUrl = @"{1}/zones/{0}/rate_limits?per_page=1000";
                var url = string.Format(requestUrl, _zoneId, _apiUrlPrefix);
                var content = HttpGet(url, 1200);
                var rules = JsonConvert.DeserializeObject<RateLimitRuleResponse>(content);
                return rules.Result;
            }
            catch (Exception)
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
            url = string.Format(url, _zoneId, _apiUrlPrefix);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPut(url, json, 90);
            updateRateLimitResponse = JsonConvert.DeserializeObject<UpdateRateLimitResponse>(content);
            return updateRateLimitResponse;
        }
        public FirewallAccessRuleResponse DeleteRateLimit(string id)
        {
            return DeleteAccessRule(_zoneId, _authEmail, _authKey, id);
            //DeleteRateLimitResponse deleteRateLimitResponse = new DeleteRateLimitResponse();
            //string url = "{2}/zones/{0}/rate_limits/{1}";
            //url = string.Format(url, _zoneId, id, _apiUrlPrefix);
            //string json = "";
            //string content = HttpDelete(url, json, 90);
            //deleteRateLimitResponse = JsonConvert.DeserializeObject<DeleteRateLimitResponse>(content);
            //return deleteRateLimitResponse;
        }

        public CloudflareRateLimitRule GetRateLimitRule(string url, int threshold, int period)
        {
            var ratelimits = GetRateLimits();
            if (null != ratelimits && ratelimits.Count > 0)
            {
                foreach (var rateLimit in ratelimits)
                {
                    if (url.Contains(rateLimit.Match.Request.Url)
                        && rateLimit.Threshold == threshold
                        && rateLimit.Period == period)
                    {
                        return rateLimit;
                    }
                }
            }
            return null;
        }

        public bool OpenRateLimit(string url, int threshold, int period, out AuditLogEntity errorLog)
        {
            var zoneTableId = ZoneBusiness.GetZoneByZoneId(_zoneId).TableID;
            errorLog = null;
            var ratelimit = GetRateLimitRule(url, threshold, period);
            if (null != ratelimit)
            {
                ratelimit.Disabled = false;
                var response = UpdateRateLimit(ratelimit);
                if (!response.success)
                {
                    errorLog = new AuditLogEntity(zoneTableId, LogLevel.Error,
                        $"Open rate limiting rule of Cloudflare failure，the reason is:[{(response.errors.Any() ? response.errors[0].message : "No error message from Cloudflare.")}].<br />");
                }
                return response.success;
            }
            else
            {
                var response = CreateRateLimit(new CloudflareRateLimitRule(url, threshold, period, "Create Rate limit rule By Attack Prevent Windows service!"));
                if (!response.success)
                {
                    errorLog = new AuditLogEntity(zoneTableId, LogLevel.Error,
                        $"Create rate limiting rule of Cloudflare failure，the reason is:[{(response.errors.Any() ? response.errors[0].message : "No error message from Cloudflare.")}].<br />");
                }
                return response.success;
            }
        }
        #endregion

        #region Ban IP
        public CloudflareAccessRuleResponse BanIp(string ip, string notes)
        {
            return CreateAccessRule(new CloudflareAccessRuleRequest(ip, "challenge", false, notes));
        }

        public CloudflareAccessRuleResponse RemoveIpFromBlacklist(string ip)
        {
            var blacklist = GetBlacklist(ip);
            if (null != blacklist && blacklist.Count > 0)
            {
                var blackInfo = blacklist[0];
                return DeleteAccessRule(blackInfo.Id);
            }
            return new CloudflareAccessRuleResponse()
            {
                Success = false,
                Errors = new string[] { "Not found in Cloudflare blacklist." }
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

        #endregion
    }
}
