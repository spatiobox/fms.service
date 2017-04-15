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
using System.IO;
using FMS.Service.Core.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Web.Hosting;
using FastReport.Web;
using System.Net.Http.Headers;
using FastReport;
using FastReport.Export.Pdf;
using FastReport.Export.OoXML;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json.Linq;

namespace FMS.Service.Controllers
{
    /// <summary>
    /// 配方
    /// </summary>
    [Authorize]
    [RoutePrefix("api")]
    public class MissionDetailsController : ApiController
    {
        private MissionDetailContext ctx = new MissionDetailContext();
        private RecipeContext ctx_recipe = new RecipeContext();
        private AuthRepository repo = new AuthRepository();
        // GET: api/MissionDetails
        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<MissionDetailData>))]
        public async Task<IHttpActionResult> GetMissionDetails([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/MissionDetails/5
        [ResponseType(typeof(MissionDetailData))]
        public async Task<IHttpActionResult> GetMissionDetail(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            MissionDetail missionDetail = await ctx.FindAsync(id);
            if (missionDetail == null)
            {
                return NotFound();
            }

            return Ok(missionDetail.ToViewData(suffix));
        }

        // PUT: api/MissionDetails/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMissionDetail(Guid id, MissionDetailData missionDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != missionDetail.ID)
            {
                return BadRequest();
            }

            try
            {
                await ctx.UpdateAsync(missionDetail.ToModel());
            }
            catch (Exception ex)
            {
                if (!MissionDetailExists(id))
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

        // POST: api/MissionDetails
        [ResponseType(typeof(MissionDetailData))]
        public async Task<IHttpActionResult> PostMissionDetail(MissionDetailData missionDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await this.User.Identity.GetUser();

            try
            {
                missionDetail.ID = Guid.NewGuid();
                missionDetail.CreateDate = DateTime.Now;
                var node = await ctx.CreateAsync(missionDetail.ToModel());
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (MissionDetailExists(missionDetail.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }

        // DELETE: api/MissionDetails/5
        [ResponseType(typeof(MissionDetail))]
        public async Task<IHttpActionResult> DeleteMissionDetail(Guid id)
        {
            var user = await this.User.Identity.GetUser();
            MissionDetail missionDetail = await ctx.FindAsync(id);
            if (missionDetail == null)
            {
                return NotFound();
            }

            List<string> msgs = new List<string>();

            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    await ctx.DeleteAsync(missionDetail);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    //var msgs = new List<string>();
                    //if (missionDetail.Recipes.Count > 0)
                    //{
                    //    msgs.Add("该配方具有{0}")
                    //}
                    tran.Rollback();
                    return BadRequest(ex.Message);
                    //throw ex;
                }
            }

            return Ok(missionDetail.ToViewData());
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

        private bool MissionDetailExists(Guid id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }



        #region Extra

        /// <summary>
        /// 更新配方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="missionDetail"></param>
        /// <returns></returns>
        [ResponseType(typeof(MissionDetailData))]
        public async Task<IHttpActionResult> PatchMissionDetail(Guid id, Newtonsoft.Json.Linq.JObject missionDetail)
        {
            try
            {
                var f = await ctx.FindAsync(id);
                if (f == null)
                {
                    return NotFound();
                }
                var node = ctx.Update(missionDetail, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!MissionDetailExists(id))
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
        /// <param name="_id">相应类型的id值</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<MissionDetailData>))]
        [Route("missiondetails/by/{category}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, string id, Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            IEnumerable<MissionDetail> list = null;
            Guid g_id = Guid.NewGuid();
            Guid.TryParse(id, out g_id);
            int i_id = -1;
            int.TryParse(id, out i_id);
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Recipe:
                        list = ctx.Filter(x => x.RecipeID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.Material:
                        list = ctx.Filter(x => x.Recipe.MaterialID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.Formular:
                        list = ctx.Filter(x => x.Mission.FormularID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.User:
                    case CategoryDictionary.Role:
                    case CategoryDictionary.MissionDetail:
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
        [Route("missionDetails/download/template")]
        public HttpResponseMessage GetTemplate()
        {
            HttpHeaderValueCollection<StringWithQualityHeaderValue> acceptedLanguages = Request.Headers.AcceptLanguage;
            var lan = "zh-CN";
            if (acceptedLanguages.Any(x => x.Value == "en-US"))
            {
                lan = "en-US";
            }
            if (acceptedLanguages.Any(x => x.Value == "zh-TW"))
            {
                lan = "zh-TW";
            }
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/missionDetails.{0}.csv", lan.ToLower()));
            string fileName = "missionDetails.csv";
            //return Ok(File(path + fileName, "text/plain", fileName)); 
            HttpResponseMessage result = null;

            if (!File.Exists(file))
            {
                result = Request.CreateResponse(HttpStatusCode.Gone);
            }
            else
            {
                // Serve the file to the client
                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(new FileStream(file, FileMode.Open, FileAccess.Read));
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/comma-separated-values");
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = fileName;
            }

            return result;
        }

        /// <summary>
        /// 检索品牌
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("missionDetails/search")]
        [ResponseType(typeof(IDaoData<MissionDetailData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(o => 1 == 1, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<MissionDetailData> { list = list, pagination = pagination });
        }

        #endregion
    }
}