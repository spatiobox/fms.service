namespace FMS.Service.DAO
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserData
    { 
        public string Id { get; set; }
         
        [StringLength(256)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        //public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string Position { get; set; }

        public int Status { get; set; }

        public string Remark { get; set; }
        

        //public virtual ICollection<ClientData> Clients { get; set; }

        public virtual ICollection<FormularData> Formulars { get; set; }

        public virtual ICollection<MaterialData> Materials { get; set; }

        public virtual ICollection<RecipeData> Recipes { get; set; }

        public virtual ICollection<RecordData> Records { get; set; }

        public virtual ICollection<RoleData> Roles { get; set; }

        public virtual ICollection<PermissionData> Permissions { get; set; }

        public virtual ProfileData Profile { get; set; }

        public virtual ICollection<OrganizationData> Organizations { get; set; }
         
    }
}
