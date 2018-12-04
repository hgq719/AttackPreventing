using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AttackPrevent.Models
{
    public class RateLimitModel
    {
        public int TableID { get; set; }

        public string ZoneId { get; set; }

        public int OrderNo { get; set; }

        [Required]
        [StringLength(50)]
        public string Url { get; set; }

        //[Required]
        //[RegularExpression(@"^[0-9]*$", ErrorMessage = "{0} Must be Integer")]
        //[Range(0, 10000, ErrorMessage = "Out of Range")]
        public int Threshold { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int Period { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int EnlargementFactor { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int RateLimitTriggerIpCount { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int RateLimitTriggerTime { get; set; }
    }
}