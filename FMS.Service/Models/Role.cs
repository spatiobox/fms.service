namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AspNetRoles")]
    public partial class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
            Permissions = new HashSet<Permission>();
        }

        public string Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; }
    }
}
