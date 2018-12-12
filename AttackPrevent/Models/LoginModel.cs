using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AttackPrevent.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "The {0} is required.")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "The {0} is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        public string verificationcode { get; set; }
    }
}