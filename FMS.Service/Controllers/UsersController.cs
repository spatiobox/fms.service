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
using FMS.Service.Core;
using System.IO;
using FMS.Service.Core.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Transactions;
using FMS.Service.DAO;

namespace FMS.Service.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Authorize(Roles = "administrator,04201795-4665-4E6C-BBF1-17F9D0B24F1E")]
    [RoutePrefix("api")]
    public class UsersController : ApiController
    {
        private UserContext ctx = new UserContext();
        private FormularContext ctx_formular = new FormularContext();
        private MaterialContext ctx_material = new MaterialContext();
        private RecipeContext ctx_recipe = new RecipeContext();
        private RoleContext ctx_role = new RoleContext();
        private AuthRepository repo = new AuthRepository();

        // GET: api/Users
        [ResponseType(typeof(IDaoData<UserData>))]
        public async Task<IHttpActionResult> GetUsers([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var list = await ctx.FilterAsync(o => 1 == 1, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Users/5
        [ResponseType(typeof(UserData))]
        public async Task<IHttpActionResult> GetUser(Guid id)
        {
            User user = await ctx.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.ToViewData());
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUser(string id, UserData user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }


            try
            {
                await ctx.UpdateAsync(user.ToModel());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        [ResponseType(typeof(UserData))]
        public async Task<IHttpActionResult> PostUser(UserData user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            try
            {
                //user.Id = Guid.NewGuid();
                var node = await ctx.CreateAsync(user.ToModel());
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (UserExists(user.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(UserData))]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin) return BadRequest("invalid_permission");
            User model = await ctx.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            if (model.Id == "0fa2be48-35ef-4341-af31-bc788b08cadb")
            {
                return BadRequest("invalid_permission");
            }

            await ctx.DeleteAsync(model);

            return Ok(model.ToViewData());
        }

        // DELETE: api/Records/5 
        [ResponseType(typeof(UserData))]
        [Route("users/batch")]
        public async Task<IHttpActionResult> DeleteUsers(CommonData node)
        {
            try
            {
                var user = await this.User.Identity.GetUser();
                var uids = node.uids;
                var isAdmin = await user.IsAdministrator();
                if (!isAdmin) return BadRequest("invalid_permission");
                //var guids = ids.Select(x => Guid.Parse(x)).ToList();
                //var guids =  ids.Split(',').Select(x => new Guid(x)).ToList();
                //var guids = list.Select(x => x.ID).ToList();

                var users = ctx.context.Users.Where(x => uids.Contains(x.Id)).ToList();
                //ctx.context.Profiles.RemoveRange(users.Select);
                foreach (var u in users)
                {
                    if (u.Id == "0fa2be48-35ef-4341-af31-bc788b08cadb")
                    {
                        return BadRequest("invalid_permission");
                    }
                    //u.UserClaims.Clear();
                    //u.UserLogins.Clear();
                    //u.Records.Clear();
                    //u.Formulars.Clear();
                    //u.Materials.Clear();
                    u.Roles.Clear();
                }
                ctx.context.Records.RemoveRange(users.SelectMany(x => x.Records));
                ctx.context.Recipes.RemoveRange(users.SelectMany(x => x.Recipes));
                ctx.context.Formulars.RemoveRange(users.SelectMany(x => x.Formulars));
                ctx.context.Materials.RemoveRange(users.SelectMany(x => x.Materials));
                ctx.context.Users.RemoveRange(users);
                await ctx.context.SaveChangesAsync();

                return Ok(uids);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repo.Dispose();
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(string id)
        {
            return ctx.Count(e => e.Id == id) > 0;
        }


        #region Extra

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [ResponseType(typeof(UserData))]
        public IHttpActionResult PatchUser(string id, Newtonsoft.Json.Linq.JObject user, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            try
            {
                var node = ctx.Update(user, id);
                return Ok(node.ToViewData(suffix));
            }
            catch (Exception ex)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 根据类型获取用户列表 
        /// </summary>
        /// <param name="category">类型</param>
        /// <param name="id">相应类型的id值</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<UserData>))]
        [Route("users/by/{category:int}/{id}")]
        public IHttpActionResult GetByCategory(CategoryDictionary category, Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            IEnumerable<User> list = null;
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Formular:
                        list = ctx_formular.Filter(x => x.ID == id).Select(x => x.User);
                        break;
                    case CategoryDictionary.Recipe:
                        list = ctx_recipe.Filter(x => x.ID == id).Select(x => x.User);
                        break;
                    case CategoryDictionary.Material:
                        list = ctx_material.Filter(x => x.ID == id).Select(x => x.User);
                        break;
                    case CategoryDictionary.Role:
                        list = ctx_role.Find(id).Users;
                        break;
                    case CategoryDictionary.User:
                    case CategoryDictionary.Permission:
                    default:
                        return BadRequest("系统不支持此功能");
                        break;
                }
            }
            catch
            {
                throw;
            }
            return Ok(list.ToList().Select(o => o.ToViewData(suffix)));
        }


        /// <summary>
        /// 下载模板
        /// </summary>
        /// <returns>Excel文档</returns>
        [Route("users/download/template")]
        public HttpResponseMessage GetTemplate()
        {

            string path = AppDomain.CurrentDomain.BaseDirectory + "assets/attachments/uploads/excel/";
            string fileName = "建筑信息Excel模版.xlsx";
            //return Ok(File(path + fileName, "text/plain", fileName));


            HttpResponseMessage result = null;
            //var localFilePath = HttpContext.Current.Server.MapPath("~/timetable.jpg");

            if (!File.Exists(path + fileName))
            {
                result = Request.CreateResponse(HttpStatusCode.Gone);
            }
            else
            {
                // Serve the file to the client
                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(new FileStream(path + fileName, FileMode.Open, FileAccess.Read));
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = fileName;
            }

            return result;
        }
        /// <summary>
        /// 导入数据
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IList<FormularData>))]
        [Route("users/import/template")]
        public IEnumerable<FormularData> PostTemplate()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "assets/attachments/uploads/excel/";
            string fileName = "建筑信息Excel模版.xlsx";
            //return Ok(File(path + fileName, "text/plain", fileName));

            HttpResponseMessage result = null;
            //var localFilePath = HttpContext.Current.Server.MapPath("~/timetable.jpg");

            if (!File.Exists(path + fileName))
            {
                result = Request.CreateResponse(HttpStatusCode.Gone);
            }
            else
            {
                // Serve the file to the client
                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(new FileStream(path + fileName, FileMode.Open, FileAccess.Read));
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = fileName;
            }

            return new List<FormularData>();
        }

        [Route("users/{id}/unlock/{status}")]
        public async Task<IHttpActionResult> PostUserUnlockout(string id, int? status = null)
        {
            var user = await repo.FindUserById(id);
            var _usr = ctx.Find(user.Id);
            _usr.Status = status.HasValue ? status.Value : 1;
            var result = ctx.Update(_usr);
            if (result != 0)
            {
                if (_usr.Status == 1)
                {
                    var lan = _usr.Profile != null && !string.IsNullOrEmpty(_usr.Profile.Language) ? _usr.Profile.Language : "zh-CN";
                    var subject = "恭喜， 您的账户已经通过审批";
                    var temp = "/Template/Mail/approval_{0}.html";
                    if (lan == "en-US")
                    {
                        lan = "en-US";
                        subject = "Congratulations, Your account has been approved";
                    }
                    if (lan == "zh-TW")
                    {
                        lan = "zh-TW";
                        subject = "恭喜, 您的帳戶已經通過審批";
                    }
                    var file = string.Format(temp, lan);
                    var body = MyConsole.ReadFile(file);
                    var domain = "www.omniteaching.com";
                    try
                    {
                        domain = MyConsole.GetAppString("domain");
                    }
                    catch (Exception)
                    {
                    }
                    EmailHelper helper = new EmailHelper(user.Email, subject, body.Replace("{username}", user.FullName ?? "").Replace("{domain}", domain));
                    helper.Send();
                }
                else
                {
                    var lan = _usr.Profile != null && !string.IsNullOrEmpty(_usr.Profile.Language) ? _usr.Profile.Language : "zh-CN";
                    var subject = "对不起， 您的账户审核不通过";
                    var temp = "/Template/Mail/approval_{0}.html";
                    if (lan == "en-US")
                    {
                        lan = "en-US";
                        subject = "Sorry, Your account review does not pass";
                    }
                    if (lan == "zh-TW")
                    {
                        lan = "zh-TW";
                        subject = "對不起, 您的帳戶審批不通過";
                    }
                    var file = string.Format(temp, lan);
                    var body = MyConsole.ReadFile(file);
                    var domain = "www.omniteaching.com";
                    try
                    {
                        domain = MyConsole.GetAppString("domain");
                    }
                    catch (Exception)
                    {
                    }
                    EmailHelper helper = new EmailHelper(user.Email, subject, body.Replace("{username}", user.FullName ?? "").Replace("{domain}", domain));
                    helper.Send();

                }
                return Ok();
            }
            return BadRequest();
        }


        [Route("users/{id}/copy/{uid}")]
        public async Task<IHttpActionResult> PostUserCopy(string id, string uid)
        {
            var source = ctx.Find(id);
            var target = ctx.Find(uid);
            IEnumerable<Guid> farr = new List<Guid>();
            IEnumerable<Guid> marr = new List<Guid>();
            IEnumerable<Guid> rarr = new List<Guid>();
            if (source != null && target != null)
            {
                using (var scope = new TransactionScope())
                {
                    try
                    {
                        var flist = source.Formulars.Where(x => !target.Formulars.Select(m => m.Title).Contains(x.Title) &&
                            !target.Formulars.Select(m => m.Code).Contains(x.Code)).ToList();
                        var fs = flist.Select(x => new Formular()
                            {
                                ID = Guid.NewGuid(),
                                Code = x.Code,
                                CreateDate = DateTime.Now,
                                Title = x.Title,
                                UserID = target.Id
                            }).ToList();
                        fs = ctx.context.Formulars.AddRange(fs).ToList();
                        ctx.context.SaveChanges();
                        farr = fs.Select(x => x.ID);

                        var mlist = source.Materials.Where(x => !target.Materials.Select(m => m.Title).Contains(x.Title) &&
                            !target.Materials.Select(m => m.Code).Contains(x.Code)
                            ).ToList();
                        var ms = mlist.Select(x => new Material()
                        {
                            ID = Guid.NewGuid(),
                            Code = x.Code,
                            Title = x.Title,
                            UserID = target.Id
                        }).ToList();
                        ms = ctx.context.Materials.AddRange(ms).ToList();
                        ctx.context.SaveChanges();
                        marr = ms.Select(x => x.ID);


                        var rlist = source.Recipes.Where(x => !target.Recipes.Any(r => r.Formular.Title == x.Formular.Title && r.Material.Title == x.Material.Title)).ToList();
                        var rs = rlist.Select(x => new Recipe
                        {
                            ID = Guid.NewGuid(),
                            Deviation = x.Deviation,
                            FormularID = fs.FirstOrDefault(f => f.Title == x.Formular.Title) == null ?
                                    target.Formulars.FirstOrDefault(f => f.Title == x.Formular.Title).ID :
                                    fs.FirstOrDefault(f => f.Title == x.Formular.Title).ID,
                            IsRatio = x.IsRatio,
                            MaterialID = ms.FirstOrDefault(m => m.Title == x.Material.Title) == null ?
                                    target.Materials.FirstOrDefault(m => m.Title == x.Material.Title).ID :
                                    ms.FirstOrDefault(m => m.Title == x.Material.Title).ID,
                            Sort = x.Sort,
                            UserID = target.Id,
                            Weight = x.Weight
                        });
                        rs = ctx.context.Recipes.AddRange(rs);
                        ctx.context.SaveChanges();
                        rarr = rs.Select(x => x.ID);

                        MyConsole.Log(
                            string.Join(",", flist.Select(x => x.ID)) + "|" +
                            string.Join(",", mlist.Select(x => x.ID)) + "|" +
                            string.Join(",", rlist.Select(x => x.ID))
                            , source.Id + " ==> " + target.Id);
                        scope.Complete();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            //var sql = "";
                            //if (rarr.Count() > 0) sql += string.Format(" DELETE FROM Recipe WHERE UserID = '{1}' AND ID in ('{0}') ", string.Join("','", rarr), target.Id);
                            //if (marr.Count() > 0) sql += string.Format(" DELETE FROM Material WHERE UserID = '{1}' AND ID in ('{0}') ", string.Join("','", marr), target.Id);
                            //if (farr.Count() > 0) sql += string.Format(" DELETE FROM Formular WHERE UserID = '{1}' AND ID in ('{0}') ", string.Join("','", farr), target.Id);
                            //if (!string.IsNullOrEmpty(sql)) ctx.context.Database.ExecuteSqlCommand(sql);
                            //MyConsole.Log(sql, "数据拷贝还原");
                            MyConsole.Log(ex);
                        }
                        catch (Exception)
                        {
                        }
                        throw ex;
                    }
                }

            }
            return BadRequest();
        }

        #endregion
    }
}