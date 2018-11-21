using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model.Cloudflare
{
    public class IpNeedToBan
    {
        public string IP { get; set; }
        public string RelatedURL { get; set; }
        public string Host { get; set; }
        public DateTime RequestedTime { get; set; }
        public bool HasBanned { get; set; }
        public string Remark { get; set; }
    }
}
