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
    public class WhiteListModel
    {
        [Required]
        [StringLength(50)]
        public string ZoneId { get; set; }

        [Required]
        [CheckIP]
        public string IP { get; set; }

        [Required]
        [StringLength(256)]
        public string Comment { get; set; }

        [Required]
        [StringLength(16)]
        [CheckValidateCode]
        public string ValidateCode { get; set; }
    }
}