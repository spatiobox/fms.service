using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.Core.Identity
{
    public class ForgotModel
    { 
        [Required]
        public string UserID { get; set; }

        [Required]
        public string Token { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
    }
}