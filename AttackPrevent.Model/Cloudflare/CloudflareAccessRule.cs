using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model.Cloudflare
{
    public class CloudflareAccessRuleListResponse
    {
        public List<CloudflareAccessRule> Result { get; set; }
        public bool Success { get; set; }
        public QueryResult Result_Info { get; set; }
    }

    public class CloudflareAccessRuleRequest
    {
        /// <summary>
        /// valid values: block, challenge, whitelist, js_challenge
        /// </summary>
        public string Mode { get; set; }
        public AccessRuleConfiguration Configuration { get; set; }
        public string Notes { get; set; }

        public CloudflareAccessRuleRequest()
        {

        }

        public CloudflareAccessRuleRequest(string ip, string mode, bool ifIpRange, string notes)
        {
            Mode = mode;
            Configuration = new AccessRuleConfiguration()
            {
                Target = ifIpRange ? "ip_range" : "ip",
                Value = ip
            };
            Notes = notes;
        }
    }

    public class CloudflareAccessRule
    {
        public string Id { get; set; }
        public bool Paused { get; set; }
        public string Modified_On { get; set; }
        public string Mode { get; set; }
        public string Notes { get; set; }
        public List<string> Allowed_Modes { get; set; }
        public string Created_On { get; set; }
        public AccessRuleConfiguration Configuration { get; set; }
        public AccessRuleScope Scope { get; set; }
    }
    
    public class AccessRuleConfiguration
    {
        public string Target { get; set; }
        public string Value { get; set; }
    }

    public class AccessRuleScope
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
    }

    public class QueryResult
    {
        public int Page { get; set; }
        public int Per_Page { get; set; }
        public int Count { get; set; }
        public int Total_Count { get; set; }
        public int Total_Pages { get; set; }
    }

    public class CloudflareAccessRuleResponse
    {
        public bool Success { get; set; }
        public AccessRuleError[] Errors { get; set; }
        public string[] Messages { get; set; }
        public CreateResult Result { get; set; }
    }

    public class AccessRuleError
    {
        public string code { get; set; }
        public string message { get; set; }

    }

    public class CreateResult
    {
        public string Id { get; set; }
        public string Notes { get; set; }
        public string[] Allowed_Modes { get; set; }
        public string Mode { get; set; }
        public AccessRuleConfiguration Configuration { get; set; }
        public string Created_On { get; set; }
        public string Modified_On { get; set; }
        public AccessRuleScope scope { get; set; }
    }
}
