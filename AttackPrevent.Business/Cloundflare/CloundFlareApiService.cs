using AttackPrevent.Model;
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

namespace AttackPrevent.Business
{
    public interface ICloundFlareApiService
    {
        Task<List<CloudflareLog>> GetCloudflareLogsAsync(DateTime start, DateTime end);
        List<CloudflareLog> GetCloudflareLogs(string zoneId, string authEmail, string authKey, double sample, DateTime start, DateTime end, out bool retry);
        List<FirewallAccessRule> GetAccessRuleList(string zoneId, string authEmail, string authKey, double sample, string ip, string notes);
        /// <summary>
        /// valid values: block, challenge, whitelist, js_challenge
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        FirewallAccessRuleResponse CreateAccessRule(string zoneId, string authEmail, string authKey, double sample, FirewallAccessRuleRequest request);
        FirewallAccessRuleResponse EditAccessRule(string zoneId, string authEmail, string authKey, double sample, string id, FirewallAccessRuleRequest request);
        FirewallAccessRuleResponse DeleteAccessRule(string zoneId, string authEmail, string authKey, double sample, string id);
        List<RateLimitRule> GetRateLimitRuleList(string zoneId, string authEmail, string authKey, double sample);
        CreateRateLimitResponse CreateRateLimit(string zoneId, string authEmail, string authKey, double sample, RateLimitRule rateLimitRule);
        UpdateRateLimitResponse UpdateRateLimit(string zoneId, string authEmail, string authKey, double sample, RateLimitRule rateLimitRule);
        DeleteRateLimitResponse DeleteRateLimit(string zoneId, string authEmail, string authKey, double sample, string id);
    }
    public class CloundFlareApiService : ICloundFlareApiService
    {
        private ILogService logger = new LogService();

        public CloundFlareApiService()
        {
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
            string content = HttpGet(authEmail, authKey, url, 1200);
            if (content.Contains("\"}"))
            {
                content = content.Replace("\"}", "\"},");
                CloudflareLogs = JsonConvert.DeserializeObject<List<CloudflareLog>>(string.Format("[{0}]", content));
            }
            else
            {
                if(content.Contains("429 Too Many Requests"))
                {
                    retry = true;
                }
                logger.Error(content);
            }
            return CloudflareLogs;
        }        
        public List<FirewallAccessRule> GetAccessRuleList(string zoneId, string authEmail, string authKey, double sample, string ip, string notes)
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

                string content = HttpGet(authEmail, authKey, url, 1200);
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
        public FirewallAccessRuleResponse CreateAccessRule(string zoneId, string authEmail, string authKey, double sample, FirewallAccessRuleRequest request)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(request);
            string content = HttpPost(authEmail, authKey, url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public FirewallAccessRuleResponse EditAccessRule(string zoneId, string authEmail, string authKey, double sample, string id, FirewallAccessRuleRequest request)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, zoneId, id);
            string json = JsonConvert.SerializeObject(request);
            string content = HttpPut(authEmail, authKey, url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public FirewallAccessRuleResponse DeleteAccessRule(string zoneId, string authEmail, string authKey, double sample, string id)
        {
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/firewall/access_rules/rules/{1}";
            url = string.Format(url, zoneId, id);
            string json = JsonConvert.SerializeObject(new { cascade = "none" });
            string content = HttpDelete(authEmail, authKey, url, json, 90);
            FirewallAccessRuleResponse response = JsonConvert.DeserializeObject<FirewallAccessRuleResponse>(content);
            return response;
        }
        public List<RateLimitRule> GetRateLimitRuleList(string zoneId, string authEmail, string authKey, double sample)
        {
            List<RateLimitRule> rateLimitRules = new List<RateLimitRule>();
            int page = 1;
            while (true)
            {
                //?page={1}&per_page={2}&notes=my note
                string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits?page={1}&per_page=20";
                url = string.Format(url, zoneId, page);               
                string content = HttpGet(authEmail, authKey, url, 1200);
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
        public CreateRateLimitResponse CreateRateLimit(string zoneId, string authEmail, string authKey, double sample, RateLimitRule rateLimitRule)
        {
            CreateRateLimitResponse createRateLimitResponse = new CreateRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPost(authEmail, authKey, url, json, 90);
            createRateLimitResponse = JsonConvert.DeserializeObject<CreateRateLimitResponse>(content);
            return createRateLimitResponse;
        }
        public UpdateRateLimitResponse UpdateRateLimit(string zoneId, string authEmail, string authKey, double sample, RateLimitRule rateLimitRule)
        {
            UpdateRateLimitResponse updateRateLimitResponse = new UpdateRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits";
            url = string.Format(url, zoneId);
            string json = JsonConvert.SerializeObject(rateLimitRule);
            string content = HttpPut(authEmail, authKey, url, json, 90);
            updateRateLimitResponse = JsonConvert.DeserializeObject<UpdateRateLimitResponse>(content);
            return updateRateLimitResponse;
        }
        public DeleteRateLimitResponse DeleteRateLimit(string zoneId, string authEmail, string authKey, double sample, string id)
        {
            DeleteRateLimitResponse deleteRateLimitResponse = new DeleteRateLimitResponse();
            string url = "https://api.cloudflare.com/client/v4/zones/{0}/rate_limits/{1}";
            url = string.Format(url, zoneId, id);
            string json = "";
            string content = HttpDelete(authEmail, authKey, url, json, 90);
            deleteRateLimitResponse = JsonConvert.DeserializeObject<DeleteRateLimitResponse>(content);
            return deleteRateLimitResponse;
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
            return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
        }
    }
}
