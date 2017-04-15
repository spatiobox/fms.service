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
using FMS.Service.DAO;
using FMS.Service.Core;
using FMS.Service.Core.Identity;
using System.Net.Http.Headers;
using System.IO;
using System.Web.Hosting;
using FastReport.Export.OoXML;
using FastReport.Export.Pdf;
using FastReport;

namespace FMS.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class ConfigsController : ApiController
    {
        private ConfigContext ctx = new ConfigContext();
        // GET: api/Configs
        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<ConfigData>))]
        public async Task<IHttpActionResult> GetConfigs([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Configs/5
        [ResponseType(typeof(ConfigData))]
        public async Task<IHttpActionResult> GetConfig(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Config dictionary = await ctx.FindAsync(id);
            if (dictionary == null)
            {
                return NotFound();
            }

            return Ok(dictionary.ToViewData(suffix));
        }

        // PUT: api/Configs/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutConfig(Guid id, ConfigData node)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != node.ID)
            {
                return BadRequest();
            }

            var user = await this.User.Identity.GetUser();

            try
            {
                await ctx.UpdateAsync(node.ToModel());
            }
            catch (Exception ex)
            {
                if (!ConfigExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw ex;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Configs
        [ResponseType(typeof(ConfigData))]
        public async Task<IHttpActionResult> PostConfig(ConfigData node)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await this.User.Identity.GetUser();
                var model = await ctx.CreateAsync(node.ToModel());
                return Ok(model.ToViewData());
            }
            catch (Exception ex)
            {
                if (ConfigExists(node.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }

        // DELETE: api/Configs/5
        [ResponseType(typeof(Config))]
        public async Task<IHttpActionResult> DeleteConfig(Guid id)
        {
            var user = await this.User.Identity.GetUser();
            Config dictionary = await ctx.FindAsync(id);

            List<string> msgs = new List<string>();


            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    ctx.context.Configs.Remove(dictionary);
                    ctx.context.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return BadRequest(ex.Message);
                }
            }

            return Ok(dictionary.ToViewData());
        }


        // DELETE: api/Records/5 
        [ResponseType(typeof(RecordData))]
        [Route("configs/batch")]
        public async Task<IHttpActionResult> DeleteConfigs(CommonData node)
        {
            var user = await this.User.Identity.GetUser();
            var ids = node.guids;
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var configs = ctx.Filter(x => ids.Contains(x.ID)).ToList();
                    if (configs.Count == 0)
                    {
                        return NotFound();
                    }
                    if (configs.Count != ids.Count)
                    {
                        var arr = ids.Where(x => !configs.Select(r => r.ID).Contains(x)).ToList();
                        return BadRequest("Configs cannot found: " + string.Join(",", arr));
                    }

                    ctx.context.Configs.RemoveRange(configs);
                    await ctx.context.SaveChangesAsync();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }

            return Ok(ids);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ConfigExists(Guid id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }



        #region Extra

        /// <summary>
        /// 更新配方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        [ResponseType(typeof(ConfigData))]
        public IHttpActionResult PatchConfig(Guid id, Newtonsoft.Json.Linq.JObject dictionary)
        {
            try
            {
                var node = ctx.Update(dictionary, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!ConfigExists(id))
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
        /// 根据类型获取配方列表 
        /// </summary>
        /// <param name="category">类型</param>
        /// <param name="id">相应类型的id值</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<ConfigData>))]
        [Route("configs/by/{category:int}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, string id, Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            IEnumerable<Config> list = null;
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Config:
                        list = ctx.Filter(x => x.AppID == id, ref pagination);
                        break;
                    case CategoryDictionary.Dictionary:
                    case CategoryDictionary.Role:
                    case CategoryDictionary.User:
                    case CategoryDictionary.Recipe:
                    case CategoryDictionary.Material:
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
        /// 检索品牌
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("configs/search")]
        [ResponseType(typeof(IDaoData<ConfigData>))] 
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(o => true, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<ConfigData> { list = list, pagination = pagination });
        }

        #endregion
    }
}