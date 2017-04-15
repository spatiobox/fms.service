using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FMS.Service.Core.Identity
{
    public class UserModel
    {
        [Key] 
        public string ID { get; set; }

        [Required]
        public string UserName { get; set; }


        public string FullName { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string Position { get; set; }
        
        public string Remark { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage="两次密码不同")]
        public string ConfirmPassword { get; set; }
    }
}