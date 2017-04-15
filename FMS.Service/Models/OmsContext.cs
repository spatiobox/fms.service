namespace FMS.Service.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class OmsContext : DbContext
    {
        public OmsContext()
            : base("name=OmsContext")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserClaim> UserClaims { get; set; }
        public virtual DbSet<UserLogin> UserLogins { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Formular> Formulars { get; set; }
        public virtual DbSet<Material> Materials { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }

        public virtual DbSet<Recipe> Recipes { get; set; }
        public virtual DbSet<Record> Records { get; set; }

        public virtual DbSet<Profile> Profiles { get; set; }

        public virtual DbSet<Bucket> Buckets { get; set; }

        public virtual DbSet<Dictionary> Dictionaries { get; set; }

        public virtual DbSet<Organization> Organizations { get; set; }

        public virtual DbSet<Mission> Missions { get; set; }

        public virtual DbSet<MissionDetail> MissionDetails { get; set; }

        public virtual DbSet<Scale> Scales { get; set; }
        public virtual DbSet<Config> Configs { get; set; }

        public virtual DbSet<TaskRecord> TaskRecords { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(e => e.Users)
                .WithMany(e => e.Roles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));


            modelBuilder.Entity<Organization>()
                .HasMany(e => e.Users)
                .WithMany(e => e.Organizations)
                .Map(m => m.ToTable("UserInOrganization").MapLeftKey("OrgID").MapRightKey("UserId"));
            //modelBuilder.Entity<Role>()
            //    .HasMany(e => e.Users)
            //    .WithMany(e => e.Roles)
            //    .Map(m => m.ToTable("Permission").MapLeftKey("PermissionId").MapRightKey("RoleId")); 


            modelBuilder.Entity<Permission>()
                .HasMany(e => e.Roles)
                .WithMany(e => e.Permissions)
                .Map(m => m.ToTable("PermissionsInRoles").MapLeftKey("PermissionId").MapRightKey("RoleId"));

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserClaims)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserLogins)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Clients)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Formulars)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Materials)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Recipes)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Records)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Profile>()
                .HasRequired(e => e.User)
                .WithRequiredPrincipal(e => e.Profile)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Formular>()
                .HasMany(e => e.Recipes)
                .WithRequired(e => e.Formular)
                .WillCascadeOnDelete(true);

            //modelBuilder.Entity<Formular>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.Formular)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Material>()
                .HasMany(e => e.Recipes)
                .WithRequired(e => e.Material)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Mission>()
                .HasMany(e => e.MissionDetails)
                .WithRequired(e => e.Mission)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<MissionDetail>()
                .HasMany(e => e.Scales)
                .WithRequired(e => e.MissionDetail)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Formular>()
                .HasMany(e => e.Missions)
                .WithRequired(e => e.Formular)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Recipe>()
                .HasMany(e => e.MissionDetails)
                .WithRequired(e => e.Recipe)
                .WillCascadeOnDelete(true);

            //modelBuilder.Entity<Material>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.Material)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Permission>()
            //    .HasMany(e => e.Roles)
            //    .WithRequired(e => e.Id)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recipe>()
                .Property(e => e.Weight)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Recipe>()
                .Property(e => e.Deviation)
                .HasPrecision(18, 4);
              
            //modelBuilder.Entity<Recipe>()
            //    .HasMany(e => e.Records)
            //    .WithRequired(e => e.Recipe)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Record>()
                .Property(e => e.Weight)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Organization>()
                .HasMany(e => e.Formulas)
                .WithRequired(e => e.Organization)
                .HasForeignKey(e => e.OrgID)
                .WillCascadeOnDelete(true);


            modelBuilder.Entity<Organization>()
                .HasMany(e => e.Children)
                .WithOptional(e => e.Parent)
                .HasForeignKey(e => e.ParentID);


            modelBuilder.Entity<MissionDetail>()
                .Property(e => e.Weight)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Scale>()
                .Property(e => e.MaxRange)
                .HasPrecision(18, 4);

        }

    }
}
