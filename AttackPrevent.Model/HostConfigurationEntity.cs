using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class HostConfigurationEntity
    {
        public int ID { get; set; }
        public string Host { get; set; }
        public int Threshold { get; set; }
        public int Period { get; set; }
        public int TableID { get; set; }
        public int ZoneTableId { get; set; }
    }
}
