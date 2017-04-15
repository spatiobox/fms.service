using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace FMS.Service.Core.Identity
{
    public class ApplicationUser : IdentityUser
    {

        public int Status { get; set; }

        [StringLength(128)]
        public string FullName { get; set; }

        [StringLength(256)]
        public string Company { get; set; }


        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Position { get; set; }

        public string Remark { get; set; }

    }

    public static class IdentityExtension
    {
        public static async Task<ApplicationUser> GetUser(this IIdentity identity)
        {
            //var repo = new AuthRepository();
            var ctx = new AuthContext();
            var _uStore = new UserStore<ApplicationUser>(ctx);
            //var userMgr = new UserManager<ApplicationUser>(_uStore);
            return await _uStore.FindByNameAsync(identity.Name); 
            //return await repo.FindUserByName(identity.Name);
        }

        public static async Task<bool> IsAdministrator(this ApplicationUser user)
        {
            return user.Roles.Any(x => x.RoleId == "04201795-4665-4E6C-BBF1-17F9D0B24F1E");
        }
    }
}