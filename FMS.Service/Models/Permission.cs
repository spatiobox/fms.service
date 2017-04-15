namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Permission")]
    public partial class Permission
    {
        public Permission()
        {
            Roles = new HashSet<Role>();
        }

        public Guid ID { get; set; }

        public int ParentID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string SystemName { get; set; }

        [Required]
        [StringLength(400)]
        public string Code { get; set; }

        [Required]
        [StringLength(256)]
        public string Controller { get; set; }

        [Required]
        [StringLength(256)]
        public string Action { get; set; }

        [Required]
        [StringLength(1024)]
        public string Url { get; set; }

        [Required]
        [StringLength(400)]
        public string Category { get; set; }

        public bool IsAPI { get; set; }

        public bool IsDefault { get; set; }

        public bool IsNav { get; set; }

        public bool Actived { get; set; }

        public int Sort { get; set; }

        [Required]
        [StringLength(2048)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Icon { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}
