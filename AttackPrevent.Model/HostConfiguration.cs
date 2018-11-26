using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class HostConfiguration
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public int Threshold { get; set; }
        public int Period { get; set; }
    }
}
