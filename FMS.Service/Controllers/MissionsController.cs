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
using System.Transactions;

namespace FMS.Service.Controllers
{
    /// <summary>
    /// 配方
    /// </summary>
    [Authorize]
    [RoutePrefix("api")]
    public class MissionsController : ApiController
    {
        private MissionContext ctx = new MissionContext();
        private RecipeContext ctx_recipe;
        private AuthRepository repo = new AuthRepository();

        public MissionsController()
        {
            ctx_recipe = new RecipeContext(ctx.context);
        }
        // GET: api/Missions
        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<MissionData>))]
        public async Task<IHttpActionResult> GetMissions([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Missions/5
        [ResponseType(typeof(MissionData))]
        public async Task<IHttpActionResult> GetMission(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Mission mission = await ctx.FindAsync(id);
            if (mission == null)
            {
                return NotFound();
            }

            return Ok(mission.ToViewData(suffix));
        }


        [Route("missions/{id}/record")]
        [ResponseType(typeof(TaskRecord))]
        public async Task<IHttpActionResult> GetTaskRecord(Guid id)
        {
            Mission mission = await ctx.FindAsync(id);
            if (mission == null)
            {
                return NotFound();
            }
            var ctxRecord = new TaskRecordContext();
            var list = ctxRecord.Filter(x => x.FormulaID == mission.FormularID).ToList();
            return Ok(list);
        }

        // PUT: api/Missions/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMission(Guid id, MissionData mission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mission.ID)
            {
                return BadRequest();
            }

            try
            {
                await ctx.UpdateAsync(mission.ToModel());
            }
            catch (Exception ex)
            {
                if (!MissionExists(id))
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

        // POST: api/Missions
        [ResponseType(typeof(MissionData))]
        public async Task<IHttpActionResult> PostMission(MissionData mission, [FromUri]Pagination pagination = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await this.User.Identity.GetUser();

            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    mission.ID = Guid.NewGuid();
                    mission.CreateDate = DateTime.Now;
                    mission.Status = 0;
                    var recipes = (await ctx_recipe.FilterAsync(x => x.FormularID == mission.FormularID, ref pagination)).ToList();
                    var list = transmitToMissionDetail(recipes, mission.ID);
                    var node = await ctx.CreateAsync(mission.ToModel());
                    ctx.context.MissionDetails.AddRange(list);
                    ctx.context.SaveChanges();

                    tran.Commit();
                    return Ok(node.ToViewData(CategoryDictionary.MissionDetail));
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    if (MissionExists(mission.ID))
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


        private List<MissionDetail> transmitToMissionDetail(List<Recipe> list, Guid missionID)
        {
            var results = new List<MissionDetail>();
            foreach (var recipe in list)
            {
                var node = new MissionDetail()
                {
                    CreateDate = DateTime.Now,
                    ID = Guid.NewGuid(),
                    MissionID = missionID,
                    RecipeID = recipe.ID,
                    Status = 0,
                    Weight = 0
                };
                results.Add(node);
            }
            return results;
        }


        // POST: api/Missions
        /// <summary>
        /// 任务派工
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mission"></param>
        /// <returns></returns>
        [ResponseType(typeof(MissionData))]
        [Route("missions/{id}/dispatch")]
        public async Task<IHttpActionResult> PostDispatchMission([FromUri]Guid id, [FromBody] MissionData mission)
        {
            var task = await ctx.FindAsync(id);
            if (task == null)
            {
                return BadRequest();
            }
            if (task.ID != mission.ID)
            {
                return BadRequest();
            }

            var ctxRecord = new TaskRecordContext();
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var records = new List<TaskRecord>();
                    foreach (var item in task.MissionDetails)
                    {
                        var bean = mission.MissionDetails.FirstOrDefault(x => x.RecipeID == item.RecipeID);
                        //如果物料已完成，或者这次没有分配（按scales来判断），直接跳过此物料的任务分配
                        if (item.Status == (int)TaskDetailStatusCategory.accomplished || bean.Scales.Count <= 0) continue;

                        var idle_scales = bean.Scales.Where(x => x.Status == (int)ScaleStatusCategory.idle);
                        if (idle_scales.Count() > 0)
                        {
                            //已分配的总重量 = 已称重量 + 正在分配（正在秤的）
                            //item.Weight += idle_scales.Where(x => x.Status == (int)ScaleStatusCategory.idle).Sum(x => x.Weight ?? 0);
                            //var total = item.Weight + idle_scales.Where(x => x.Status == (int)ScaleStatusCategory.idle).Sum(x => x.Weight ?? 0);
                            var deviation = item.Recipe.IsRatio ? (item.Recipe.Deviation * item.Recipe.Weight / 100) : item.Recipe.Deviation;
                            //var diff = Math.Abs(total - item.Recipe.Weight) <= deviation;
                            item.CreateDate = DateTime.Now;
                            item.Status = (int)TaskDetailStatusCategory.weighing;

                            ////变更 台秤状态
                            var scale_ids = idle_scales.Select(x => x.ID);//bean.Scales.Where(x => !item.Scales.Select(s => s.ID).Contains(x.ID)).Select(x => x.ID);
                            var scales = ctx.context.Scales.Where(x => scale_ids.Contains(x.ID)).ToList();

                            foreach (var scale in scales)
                            {
                                scale.MissionDetailID = item.ID;
                                scale.Weight = bean.Scales.FirstOrDefault(x => x.ID == scale.ID).Weight;
                                scale.DeviationWeight = deviation / scales.Count;
                                scale.Status = (int)ScaleStatusCategory.working;
                                scale.Salt = Guid.NewGuid();
                                //item.Weighing += scale.Weight ?? 0;


                                var record = new TaskRecord();
                                record.FormulaID = item.Recipe.FormularID;
                                record.RecipeID = item.RecipeID;
                                record.ScaleID = scale.ID;
                                record.Weight = scale.Weight ?? 0;
                                record.DeviationWeight = scale.DeviationWeight ?? 0;
                                records.Add(record);
                            }
                            ctx.context.SaveChanges();
                            //item.Scales = item.Scales.Concat(scales).ToList();
                        }
                    }
                    task.Status = (int)TaskStatusCategory.working;
                    ctx.context.SaveChanges();
                    var list = ctx.context.TaskRecords.Where(x => x.FormulaID == task.FormularID);
                    ctx.context.TaskRecords.RemoveRange(list);
                    ctx.context.TaskRecords.AddRange(records);
                    ctx.context.SaveChanges();

                    tran.Commit();
                    //task = await ctx.FindAsync(id);
                    return Ok(task.ToViewData(CategoryDictionary.MissionDetail));
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MyConsole.Log(ex, "任务分配异常");
                    return BadRequest();
                }
            }

        }

        // POST: api/Missions
        /// <summary>
        /// 派工监测
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mission"></param>
        /// <returns></returns>
        [ResponseType(typeof(MissionData))]
        [Route("missions/{id}/spy")]
        public async Task<IHttpActionResult> PostSpyMission(Guid id, [FromBody] CommonData data)
        {
            var ctxTaskDetail = new MissionDetailContext();
            var detail = await ctxTaskDetail.FindAsync(id);
            if (detail == null)
            {
                return BadRequest();
            }
            var list = detail.Scales.Select(x => x.ID).ToList();
            IEnumerable<ScaleData> _list_scale;
            if (data != null && data.guids != null)
            {
                _list_scale = ctx.context.Scales.Where(x => data.guids.Contains(x.ID)).ToViewList();
            }
            else
            {
                _list_scale = new List<ScaleData>();
            }
            //task = await ctx.FindAsync(id);
            return Ok(new
            {
                Model = detail.ToViewData(CategoryDictionary.Scale),
                Scales = _list_scale
            });
        }


        // DELETE: api/Missions/5
        [ResponseType(typeof(Mission))]
        public async Task<IHttpActionResult> DeleteMission(Guid id)
        {
            //var user = await this.User.Identity.GetUser();
            Mission mission = await ctx.FindAsync(id);
            if (mission == null)
            {
                return NotFound();
            }

            List<string> msgs = new List<string>();

            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var scales = mission.MissionDetails.SelectMany(x => x.Scales);
                    if (scales.Count() > 0)
                    {
                        foreach (var item in scales)
                        {
                            //item.Status = (int)ScaleStatusCategory.cancel;
                            item.Status = (int)ScaleStatusCategory.idle;
                            item.Salt = null;
                            item.Weight = null;
                            item.MissionDetailID = null;
                        }
                        //mission.Status = (int)TaskStatusCategory.cancel;
                        ctx.context.SaveChanges();
                    }
                    //else
                    //{
                    ctx.Delete(mission);
                    //}
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    //var msgs = new List<string>();
                    //if (mission.Recipes.Count > 0)
                    //{
                    //    msgs.Add("该配方具有{0}")
                    //}
                    tran.Rollback();
                    return BadRequest(ex.Message);
                    //throw ex;
                }
            }

            return Ok(mission.ToViewData());
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

        private bool MissionExists(Guid id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }



        #region Extra

        /// <summary>
        /// 更新配方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mission"></param>
        /// <returns></returns>
        [ResponseType(typeof(MissionData))]
        public async Task<IHttpActionResult> PatchMission(Guid id, Newtonsoft.Json.Linq.JObject mission)
        {
            try
            {
                var f = await ctx.FindAsync(id);
                if (f == null)
                {
                    return NotFound();
                }
                var node = ctx.Update(mission, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!MissionExists(id))
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
        [ResponseType(typeof(IEnumerable<MissionData>))]
        [Route("missions/by/{category}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, string id, Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            IEnumerable<Mission> list = null;
            Guid g_id = Guid.NewGuid();
            Guid.TryParse(id, out g_id);
            int i_id = -1;
            int.TryParse(id, out i_id);
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Recipe:
                        list = ctx.Filter(x => x.MissionDetails.Any(md => md.RecipeID == g_id), ref pagination);
                        break;
                    case CategoryDictionary.Material:
                        list = ctx.Filter(x => x.MissionDetails.Any(md => md.Recipe.Material.ID == g_id), ref pagination);
                        break;
                    case CategoryDictionary.Formular:
                        list = ctx.Filter(x => x.FormularID == g_id, ref pagination);
                        break;
                    case CategoryDictionary.User:
                    case CategoryDictionary.Role:
                    case CategoryDictionary.Permission:
                    case CategoryDictionary.Mission:
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
        [Route("missions/download/template")]
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
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/missions.{0}.csv", lan.ToLower()));
            string fileName = "missions.csv";
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
        [Route("missions/search")]
        [ResponseType(typeof(IDaoData<MissionData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(o => 1 == 1, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<MissionData> { list = list, pagination = pagination });
        }

        #endregion
    }
}