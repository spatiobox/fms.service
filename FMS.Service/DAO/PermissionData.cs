namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PermissionData
    {
        public Guid ID { get; set; }

        public int ParentID { get; set; }
         
        public string Name { get; set; }

        public string SystemName { get; set; }
         
        public string Code { get; set; }
         
        public string Controller { get; set; }
          
        public string Action { get; set; }
         
        public string Url { get; set; }
         
        public string Category { get; set; }

        public bool IsDefault { get; set; }

        public bool IsAPI { get; set; }

        public bool IsNav { get; set; }

        public bool Actived { get; set; }

        public int Sort { get; set; }
         
        public string Description { get; set; }
         
        public string Icon { get; set; }

        public virtual ICollection<RoleData> Roles { get; set; }
    }
}
