using AttackPrevent.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AttackPrevent.Models
{
    public class ValidateCodeModel
    {
        [Required]
        [StringLength(16)]
        [CheckValidateCode]
        [Display(Name = "Validate Code")]
        public string ValidateCode { get; set; }
    }
}