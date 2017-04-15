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
    public class ScalesController : ApiController
    {
        private ScaleContext ctx = new ScaleContext();
        //private MissionContext ctxTask = new MissionContext(ctx.context);
        private AuthRepository repo = new AuthRepository();
        // GET: api/Scales
        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<ScaleData>))]
        public async Task<IHttpActionResult> GetScales([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Scales/5
        [ResponseType(typeof(ScaleData))]
        public async Task<IHttpActionResult> GetScale(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Scale scale = await ctx.FindAsync(id);
            if (scale == null)
            {
                return NotFound();
            }

            return Ok(scale.ToViewData(suffix));
        }


        [ResponseType(typeof(ScaleData))]
        [Route("scales/{id}/tick")]
        public async Task<IHttpActionResult> GetScaleTick(Guid id, [FromUri]int? status = null, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Scale scale = await ctx.FindAsync(id);
            if (scale == null)
            {
                return NotFound();
            }
            //sacle
            scale.LastHeartBeat = DateTime.Now;
            if (scale.Status == (int)ScaleStatusCategory.offline)
            {
                scale.Status = (int)ScaleStatusCategory.idle;
            }
            if (status.HasValue)
            {
                scale.Status = status.Value;
            }
            await ctx.UpdateAsync(scale);


            return Ok(scale.ToViewData(suffix));
        }

        [ResponseType(typeof(ScaleData))]
        [Route("scales/{id}/tasks")]
        public async Task<IHttpActionResult> GetScaleTasks(Guid id, [FromUri]int? status = null, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            /**
             * 客户端心跳包，心跳包的作用： 
             * 1） 台秤处于离线状态，则告诉系统进入空闲状态
             * 2） 台秤处于取消状态，则告诉系统取消成功，仍处于取消状态
             * 3） 台秤处于空闲状态，则获取任务信息
             * 4） 台秤处于称重状态，则获取任务信息
             * 5） 台秤收到取消命令, 则反馈取消成功
             * 6） 台秤收到暂停命令，则反馈暂停成功
             * 7） 台秤收到称重命令，则反馈称重成功
             * 服务器端心跳包反馈：
             * 1） 下发称重指令，包含任务信息，客户端进入称重状态，任务完成后，客户端反馈，进入空闲状态
             * 2） 下发暂停指令，客户端反馈后，进入暂停状态
             * 3） 下发取消指令，客户端反馈后，进入空闲状态
             * 4） 下发继续指令，客户端进入称重状态
             * 
             */
            Scale scale = await ctx.FindAsync(id);
            if (scale == null)
            {
                return NotFound();
            }

            //sacle
            //如果是离线状态，则标识scale为空闲
            if (((DateTime.Now - scale.LastHeartBeat).TotalMinutes > 30) || scale.Status == (int)ScaleStatusCategory.offline)
            {
                scale.LastHeartBeat = DateTime.Now;
                if (scale.Status == (int)ScaleStatusCategory.offline)
                    scale.Status = (int)ScaleStatusCategory.idle;
                ctx.context.SaveChanges();
            }

            return Ok(scale.ToViewData(suffix));
        }

        // PUT: api/Scales/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutScale(Guid id, ScaleData scale)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scale.ID)
            {
                return BadRequest();
            }

            try
            {
                await ctx.UpdateAsync(scale.ToModel());
            }
            catch (Exception ex)
            {
                if (!ScaleExists(id))
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

        // POST: api/Scales
        [ResponseType(typeof(ScaleData))]
        public async Task<IHttpActionResult> PostScale(ScaleData scale)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            if (string.IsNullOrEmpty(scale.Device))
            {
                return BadRequest();
            }

            try
            {
                var model = ctx.context.Scales.FirstOrDefault(x => x.Device == scale.Device);
                if (model == null)
                {
                    model = scale.ToModel();
                    model.ID = Guid.NewGuid();
                    model.LastHeartBeat = DateTime.Now;
                    model.Status = 1;
                    var node = await ctx.CreateAsync(model);
                    return Ok(node.ToViewData());
                }
                else
                {
                    model.LastHeartBeat = DateTime.Now;
                    if (scale.Status == (int)ScaleStatusCategory.offline)
                        scale.Status = (int)ScaleStatusCategory.idle;
                    ctx.context.SaveChanges();
                    return Ok(model.ToViewData());
                }
            }
            catch (Exception ex)
            {
                if (ScaleExists(scale.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }


        // POST: api/Scales
        [ResponseType(typeof(ScaleData))]
        [Route("scales/{id}/tasks")]
        public async Task<IHttpActionResult> PostScaleTasks(Guid id, ScaleData scale)
        {

            if (string.IsNullOrEmpty(scale.Device))
            {
                return BadRequest("error_machine_identifier");
            }
            var model = await ctx.context.Scales.FirstOrDefaultAsync(x => x.ID == id && x.Device == scale.Device && x.MissionDetailID == scale.MissionDetailID && x.Salt == scale.Salt);
            if (model == null)
            {
                return NotFound();
            }


            //取消成功
            if (model.Status == (int)ScaleStatusCategory.cancel)
            {

                using (var tran = ctx.context.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.MissionDetail.Mission.Status == (int)TaskStatusCategory.cancel)
                        {
                            if (model.MissionDetail.Mission.MissionDetails.SelectMany(x => x.Scales).Count() == 0)
                            {
                                ctx.context.Missions.Remove(model.MissionDetail.Mission);
                            }
                        }

                        model.Status = (int)ScaleStatusCategory.idle;
                        model.MissionDetailID = null;
                        model.Salt = null;
                        model.DeviationWeight = null;
                        model.Weight = null;
                        model.LastHeartBeat = DateTime.Now;


                        ctx.context.SaveChanges();

                        tran.Commit();
                        return Ok(model.ToViewData());
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        if (ScaleExists(scale.ID))
                        {
                            return Conflict();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            //暂停成功
            //if (model.Status == (int)ScaleStatusCategory.cancel)
            //{

            //}

            if (scale.Weight == 0)
            {
                return BadRequest("not_in_weight_or_donot_weight_anything");
            }

            var user = await this.User.Identity.GetUser();
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var recode = new Record()
                    {
                        OrgID = 0,
                        Device = scale.Device,
                        BatchNo = scale.MissionID.ToString(),
                        Copies = 1,
                        FormularID = model.MissionDetail.Mission.FormularID,
                        FormularCode = model.MissionDetail.Mission.Formular.Code,
                        FormularTitle = model.MissionDetail.Mission.Formular.Title,
                        MaterialCode = model.MissionDetail.Recipe.Material.Code,
                        MaterialID = model.MissionDetail.Recipe.FormularID,
                        MaterialTitle = model.MissionDetail.Recipe.Material.Title,
                        OrgTitle = "task",
                        RecipeID = model.MissionDetail.RecipeID,
                        RecordDate = DateTime.Now,
                        StandardWeight = model.MissionDetail.Recipe.Weight,
                        Weight = scale.Weight ?? 0,
                        UserID = user.Id,
                        Viscosity = "",
                        ID = Guid.NewGuid()
                    };
                    ctx.context.Records.Add(recode);

                    //misiondetail
                    model.MissionDetail.Weight += scale.Weight ?? 0;
                    var deviation = model.MissionDetail.Recipe.IsRatio ? (model.MissionDetail.Recipe.Deviation * model.MissionDetail.Recipe.Weight / 100) : model.MissionDetail.Recipe.Deviation;
                    //如果 (现在称过来的重量 + 已经称完的重量 >= 标准重量), 则表示任务完成 
                    //如果 已称完的重量 - 标准重量 <= 误差重量，则表示任务明细已完成
                    if (Math.Abs(model.MissionDetail.Weight - model.MissionDetail.Recipe.Weight) <= deviation
                        || model.MissionDetail.Weight >= model.MissionDetail.Recipe.Weight)
                    {
                        model.MissionDetail.Status = (int)TaskDetailStatusCategory.accomplished;
                    }
                    //如果配方单都已经完成，则标识配方单完成
                    if (!model.MissionDetail.Mission.MissionDetails.Any(x => x.Status != (int)TaskStatusCategory.accomplished))
                    {
                        model.MissionDetail.Mission.Status = (int)TaskStatusCategory.accomplished;
                    }
                    model.Status = (int)ScaleStatusCategory.idle;
                    model.MissionDetailID = null;
                    model.Salt = null;
                    model.DeviationWeight = null;
                    model.Weight = null;

                    ctx.context.SaveChanges();
                    tran.Commit();
                    return Ok(model.ToViewData());
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    if (ScaleExists(scale.ID))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }

        // DELETE: api/Scales/5
        [ResponseType(typeof(Scale))]
        public async Task<IHttpActionResult> DeleteScale(Guid id)
        {
            var user = await this.User.Identity.GetUser();
            Scale scale = await ctx.FindAsync(id);
            if (scale == null)
            {
                return NotFound();
            }

            List<string> msgs = new List<string>();

            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    await ctx.DeleteAsync(scale);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    //var msgs = new List<string>();
                    //if (scale.Recipes.Count > 0)
                    //{
                    //    msgs.Add("该配方具有{0}")
                    //}
                    tran.Rollback();
                    return BadRequest(ex.Message);
                    //throw ex;
                }
            }

            return Ok(scale.ToViewData());
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

        private bool ScaleExists(Guid id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }



        #region Extra

        /// <summary>
        /// 更新配方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        [ResponseType(typeof(ScaleData))]
        public async Task<IHttpActionResult> PatchScale(Guid id, Newtonsoft.Json.Linq.JObject scale)
        {
            try
            {
                var f = await ctx.FindAsync(id);
                if (f == null)
                {
                    return NotFound();
                }
                var node = ctx.Update(scale, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!ScaleExists(id))
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
        [ResponseType(typeof(IEnumerable<ScaleData>))]
        [Route("scales/by/{category}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, string id, Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            IEnumerable<Scale> list = null;
            Guid g_id = Guid.NewGuid();
            Guid.TryParse(id, out g_id);
            int i_id = -1;
            int.TryParse(id, out i_id);
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Recipe:
                        list = ctx.Filter(x => x.MissionDetailID.HasValue && x.MissionDetail.RecipeID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.Material:
                        list = ctx.Filter(x => x.MissionDetailID.HasValue && x.MissionDetail.Recipe.MaterialID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.Formular:
                        list = ctx.Filter(x => x.MissionDetailID.HasValue && x.MissionDetail.Mission.FormularID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.User:
                    case CategoryDictionary.Role:
                    case CategoryDictionary.Scale:
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
        [Route("scales/download/template")]
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
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/scales.{0}.csv", lan.ToLower()));
            string fileName = "scales.csv";
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
        [Route("scales/search")]
        [ResponseType(typeof(IDaoData<ScaleData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(o => 1 == 1, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<ScaleData> { list = list, pagination = pagination });
        }

        #endregion
    }
}