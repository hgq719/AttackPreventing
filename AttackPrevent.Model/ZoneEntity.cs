using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class ZoneEntity
    {
        public string ZoneId { get; set; }

        public string ZoneName { get; set; }

        public string AuthEmail { get; set; }

        public bool IfTestStage { get; set; }

        public bool IfEnable { get; set; }

        public bool IfAttacking { get; set; }
    }
}
