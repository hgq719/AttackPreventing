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
        [DataType(DataType.Url)]
        public string Url { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Must be Number")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Must be Number")]
        public int Threshold { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Must be Number")]
        public int Period { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Must be Number")]
        public int EnlargementFactor { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Must be Number")]
        public int RateLimitTriggerIpCount { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Must be Number")]
        public int RateLimitTriggerTime { get; set; }
    }
}