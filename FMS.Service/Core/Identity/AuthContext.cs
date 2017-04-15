using Microsoft.AspNet.Identity.EntityFramework;
using FMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FMS.Service.Core.Identity
{
    public class AuthContext : IdentityDbContext<ApplicationUser>
    {
        public AuthContext()
            : base("OmsContext")
        {

        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<User>().ToTable("User");//.Property(x => x.Id).HasColumnName("UserID");
            //modelBuilder.Entity<UserClaim>().ToTable("UserClaim");//.Property(x => x.Id).HasColumnName("UserID");
            //modelBuilder.Entity<UserLogin>().ToTable("UserLogin");
            //modelBuilder.Entity<Role>().ToTable("Roles");
        }
    }
}