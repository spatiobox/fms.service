using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FMS.Service.Models
{
    [Table("Organization")]
    public class Organization
    {
        public Organization() {
            Children = new HashSet<Organization>();
            Formulas = new HashSet<Formular>();
            Users = new HashSet<User>();
        }

        public int ID { get; set; }

        public int? ParentID { get; set; }

        [StringLength(50)]
        public string Title { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public virtual Organization Parent { get; set; }

        public virtual ICollection<Organization> Children { get; set; }

        public virtual ICollection<Formular> Formulas { get; set; }
    }
}