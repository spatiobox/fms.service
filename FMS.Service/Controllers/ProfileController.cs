using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FMS.Service.Core.Identity;
using System.Threading.Tasks;
using FMS.Service.Models;
using FMS.Service.Core;
using FMS.Service.DAO;

namespace FMS.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class ProfileController : ApiController
    {
        public async Task<IHttpActionResult> Get()
        {
            var repo = new AuthRepository();

            var ctx = new OmsContext();
            try
            {
                var user = await this.User.Identity.GetUser();
                var _usr = ctx.Users.Find(user.Id);
                var roles = user.Roles.Select(x => x.RoleId);
                var node = new
                {
                    User = new
                    {
                        Id = user.Id,
                        Name = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Status = user.Status
                    },
                    Language = _usr.Profile != null && !string.IsNullOrEmpty(_usr.Profile.Language) ? _usr.Profile.Language : "zh-CN",
                    Roles = ctx.Roles.Where(x => roles.Contains(x.Id)).Select(x => new { Id = x.Id, Name = x.Name }).ToList(),
                    Permissions = ctx.Permissions.Where(x => x.Roles.Any(r => roles.Contains(r.Id))).Distinct().OrderBy(x => x.Sort).ToList().Select(x => x.ToViewData()).ToList()
                };
                return Ok(node);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task<IHttpActionResult> Post(ProfileData node)
        {
            var ctx = new OmsContext();
            try
            {
                var user = await this.User.Identity.GetUser();
                var _usr = ctx.Users.Find(user.Id);
                var profile = _usr.Profile;
                if (profile == null)
                {
                    profile = new Profile()
                    {
                        UserID = _usr.Id,
                        Language = node.Language ?? "zh-CN"
                    };
                    ctx.Profiles.Add(profile);
                    ctx.SaveChanges();
                }
                else
                {
                    profile.Language = node.Language ?? "zh-CN";
                    ctx.Entry<Profile>(profile).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
                return Ok(profile.ToViewData());
            }
            catch (Exception ex)
            {

                throw ex;

            }

        }
    }
}
