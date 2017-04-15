using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FMS.Service.Models;
using FMS.Service.Core.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using FMS.Service.DAO;
using FMS.Service.Core;

namespace FMS.Service.Controllers
{
    /// <summary>
    /// 权限
    /// </summary>
    [Authorize(Roles = "administrator,04201795-4665-4E6C-BBF1-17F9D0B24F1E")]
    [RoutePrefix("api")]
    public class PermissionsController : ApiController
    {
        private OmsContext db = new OmsContext();
        private PermissionContext ctx = new PermissionContext();
        private AuthRepository repo = new AuthRepository();

        // GET: api/Permissions
        [ResponseType(typeof(IDaoData<FormularData>))]
        public async Task<IHttpActionResult> GetPermissions([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)).OrderBy(x => x.Sort), pagination = pagination });
        }

        // GET: api/Permissions/5
        [ResponseType(typeof(Permission))]
        public async Task<IHttpActionResult> GetPermission(Guid id)
        {
            Permission permission = await db.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            return Ok(permission);
        }

        // PUT: api/Permissions/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutPermission(Guid id, Permission permission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != permission.ID)
            {
                return BadRequest();
            }

            db.Entry(permission).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermissionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Permissions
        [ResponseType(typeof(Permission))]
        public async Task<IHttpActionResult> PostPermission(Permission permission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Permissions.Add(permission);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PermissionExists(permission.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = permission.ID }, permission);
        }

        // DELETE: api/Permissions/5
        [ResponseType(typeof(Permission))]
        public async Task<IHttpActionResult> DeletePermission(Guid id)
        {
            Permission permission = await db.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            db.Permissions.Remove(permission);
            await db.SaveChangesAsync();

            return Ok(permission);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repo.Dispose();
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PermissionExists(Guid id)
        {
            return db.Permissions.Count(e => e.ID == id) > 0;
        }
         
    }
}