using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class ActionReport
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ZoneId { get; set; }
        public string IP { get; set; }
        public string HostName { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
        public int Avg { get; set; }
        public int Count { get; set; }
        public string FullUrl { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Mode { get; set; } = "Action";
        public string Remark { get; set; }
        public string MaxDisplay { get; set; }
        public string MinDisplay { get; set; }
        public string AvgDisplay { get; set; }
    }
}
