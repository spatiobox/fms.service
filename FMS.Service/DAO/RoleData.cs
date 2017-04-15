namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RoleData
    {
        //public RoleData()
        //{
        //    Permissions = new HashSet<PermissionData>(); 
        //}

        public string ID { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        //[Required]
        //[StringLength(500)]
        //public string Description { get; set; }

        public virtual ICollection<UserData> Users { get; set; }
         
        public virtual ICollection<PermissionData> Permissions { get; set; }
         
    }
}
