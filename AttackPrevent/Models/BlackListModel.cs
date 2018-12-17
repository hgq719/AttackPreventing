using AttackPrevent.Business;
using AttackPrevent.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttackPrevent.Models
{
    public class BlackListModel
    {
        [Required]
        public int ZoneTableId { get; set; }

        [Required]
        [CheckIP]
        public string IP { get; set; }

        [Required]
        [StringLength(256)]
        public string Comment { get; set; }

        [Required]
        [StringLength(16)]
        [CheckValidateCode]
        [Display(Name ="Validate Code")]
        public string ValidateCode { get; set; }
    }
}