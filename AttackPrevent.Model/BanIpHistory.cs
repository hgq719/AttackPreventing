using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class BanIpHistory
    {
        public int Id { get; set; }
        public int ZoneTableId { get; set; }
        public string IP { get; set; }
        public DateTime LatestTriggerTime { get; set; }
        public int RuleId { get; set; }
        public string Remark { get; set; }
    }
}
