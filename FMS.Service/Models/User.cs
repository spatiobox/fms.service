namespace FMS.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AspNetUsers")]
    public partial class User
    {
        public User()
        {
            UserClaims = new HashSet<UserClaim>();
            UserLogins = new HashSet<UserLogin>();
            Clients = new HashSet<Client>();
            Formulars = new HashSet<Formular>();
            Materials = new HashSet<Material>();
            Recipes = new HashSet<Recipe>();
            Records = new HashSet<Record>();
            Roles = new HashSet<Role>();
            Organizations = new HashSet<Organization>();
        }

        public string Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        [Required]
        [StringLength(256)]
        public string UserName { get; set; }

        public int Status { get; set; }

        public string FullName { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string Position { get; set; }

        public string Remark { get; set; }

        public virtual ICollection<UserClaim> UserClaims { get; set; }

        public virtual ICollection<UserLogin> UserLogins { get; set; }

        public virtual ICollection<Client> Clients { get; set; }

        public virtual ICollection<Formular> Formulars { get; set; }

        public virtual ICollection<Material> Materials { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }

        public virtual ICollection<Record> Records { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        public virtual Profile Profile { get; set; }

        public virtual ICollection<Organization> Organizations { get; set; }
         
    }
}
