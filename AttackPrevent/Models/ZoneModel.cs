using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AttackPrevent.Models
{
    public class ZoneModel
    {
        public int TableID { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [StringLength(50)]
        public string ZoneId { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [StringLength(50)]
        public string ZoneName { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [EmailAddress]
        [StringLength(50)]
        public string AuthEmail { get; set; }

        [JsonIgnore]
        [Required(ErrorMessage = "The {0} is required.")]
        [StringLength(50)]
        public string AuthKey { get; set; }
        public bool IfTestStage { get; set; }
        public bool IfEnable { get; set; }

        public bool IfAttacking { get; set; }

    }
}