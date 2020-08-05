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

        [Required(ErrorMessage = "The {0} is required.")]
        [StringLength(100)]
        public string Url { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        //[RegularExpression(@"^[0-9]*$", ErrorMessage = "{0} Must be Integer")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int Threshold { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int Period { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public float EnlargementFactor { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int RateLimitTriggerIpCount { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int RateLimitTriggerTime { get; set; }

        public bool IfTesting { get; set; } = false;

        public bool IfOpenRateLimitRule { get; set; }

        public bool IfBanIp { get; set; }
    }
}