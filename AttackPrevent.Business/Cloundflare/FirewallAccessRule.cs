using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class FirewallAccessRule
    {
        public string id { get; set; }
        public string notes { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumMode mode { get; set; }
        public string configurationTarget { get; set; }
        public string configurationValue { get; set; }
        public DateTime createTime { get; set; }
        public DateTime modifiedTime { get; set; }
        public string scopeId { get; set; }
        public string scopeEmail { get; set; }
        public string scopeType { get; set; }
    }
    public enum EnumMode
    {
        block,
        challenge,
        whitelist,
        js_challenge
    }
}
