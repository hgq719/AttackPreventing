using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class WhiteListModel
    {
        public string IP { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreateTime { get; set; }
        public string Notes { get; set; }
    }
}
