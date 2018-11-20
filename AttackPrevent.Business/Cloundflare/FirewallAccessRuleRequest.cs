using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class FirewallAccessRuleRequest
    {
        /// <summary>
        /// valid values: block, challenge, whitelist, js_challenge
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumMode mode { get; set; }
        public Configuration configuration { get; set; }
        public string notes { get; set; }
    }
    public class Configuration
    {
        public string target { get; set; }
        public string value { get; set; }
    }
    public class FirewallAccessRuleResponse
    {
        public bool success { get; set; }
        public Error[] errors { get; set; }
        public string[] messages { get; set; }
        public CreateResult result { get; set; }
    }
    public class FirewallAccessRuleResponseList
    {
        public bool success { get; set; }
        public Error[] errors { get; set; }
        public string[] messages { get; set; }
        public CreateResult[] result { get; set; }
        public resultInfo result_info { get; set; }
    }
    public class resultInfo
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int count { get; set; }
        public int total_count { get; set; }
        public int total_pages { get; set; }
    }
    public class Error
    {
        public string message { get; set; }
    }
    public class CreateResult
    {
        public string id { get; set; }
        public string notes { get; set; }
        public string[] allowed_modes { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumMode mode { get; set; }
        public Configuration configuration { get; set; }
        public DateTime created_on { get; set; }
        public DateTime modified_on { get; set; }
        public Scope scope { get; set; }
    }
    public class Scope
    {
        public string id { get; set; }
        public string email { get; set; }
        public string type { get; set; }
    }
}
