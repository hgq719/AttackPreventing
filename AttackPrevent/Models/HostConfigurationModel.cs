using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AttackPrevent.Models
{
    public class HostConfigurationModel
    {
        public int TableID { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [StringLength(100)]
        public string Host { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int Threshold { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [Range(0, 10000, ErrorMessage = "Out of Range")]
        public int Period { get; set; }
    }
}