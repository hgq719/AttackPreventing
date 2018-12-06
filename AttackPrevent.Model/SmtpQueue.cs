using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class SmtpQueue
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }       
        public DateTime CreatedTime { get; set; }
        public DateTime SendedTime { get; set; }
        public string Remark { get; set; }
    }
}
