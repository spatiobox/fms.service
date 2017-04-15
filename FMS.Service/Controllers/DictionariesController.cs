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
    public class DictionariesController : ApiController
    {
        private DictionaryContext ctx = new DictionaryContext();
        // GET: api/Dictionaries
        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<DictionaryData>))]
        public async Task<IHttpActionResult> GetDictionaries([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.OrderBy(x => x.Code).ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Dictionaries/5
        [ResponseType(typeof(DictionaryData))]
        public async Task<IHttpActionResult> GetDictionary(int id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Dictionary dictionary = await ctx.FindAsync(id);
            if (dictionary == null)
            {
                return NotFound();
            }

            return Ok(dictionary.ToViewData(suffix));
        }

        // PUT: api/Dictionaries/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutDictionary(int id, DictionaryData dictionary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != dictionary.ID)
            {
                return BadRequest();
            }

            var user = await this.User.Identity.GetUser();

            try
            {
                await ctx.UpdateAsync(dictionary.ToModel());
            }
            catch (Exception ex)
            {
                if (!DictionaryExists(id))
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

        // POST: api/Dictionaries
        [ResponseType(typeof(DictionaryData))]
        public async Task<IHttpActionResult> PostDictionary(DictionaryData dictionary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await this.User.Identity.GetUser();
                var node = await ctx.CreateAsync(dictionary.ToModel());
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (DictionaryExists(dictionary.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }

        // DELETE: api/Dictionaries/5
        [ResponseType(typeof(Dictionary))]
        public async Task<IHttpActionResult> DeleteDictionary(int id)
        {
            var user = await this.User.Identity.GetUser();
            Dictionary dictionary = await ctx.FindAsync(id);

            List<string> msgs = new List<string>();


            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    ctx.context.Dictionaries.Remove(dictionary);
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
        [Route("dictionaries/batch")]
        public async Task<IHttpActionResult> DeleteDictionaries(CommonData node)
        {
            var user = await this.User.Identity.GetUser();
            var ids = node.ids;
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var dictionaries = ctx.Filter(x => ids.Contains(x.ID)).ToList();
                    if (dictionaries.Count == 0)
                    {
                        return NotFound();
                    }
                    if (dictionaries.Count != ids.Count)
                    {
                        var arr = ids.Where(x => !dictionaries.Select(r => r.ID).Contains(x)).ToList();
                        return BadRequest("Dictionaries cannot found: " + string.Join(",", arr));
                    }

                    ctx.context.Dictionaries.RemoveRange(dictionaries);
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

        private bool DictionaryExists(int id)
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
        [ResponseType(typeof(DictionaryData))]
        public IHttpActionResult PatchDictionary(int id, Newtonsoft.Json.Linq.JObject dictionary)
        {
            try
            {
                var node = ctx.Update(dictionary, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!DictionaryExists(id))
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
        [ResponseType(typeof(IEnumerable<DictionaryData>))]
        [Route("dictionaries/by/{category:int}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, string id, Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            IEnumerable<Dictionary> list = null;
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Dictionary:
                        list = ctx.Filter(x => x.Code == id);
                        break;
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
        /// 下载模板
        /// </summary>
        /// <returns>Excel文档</returns>
        [Route("dictionaries/download/template")]
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
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/dictionaries.{0}.csv", lan.ToLower()));
            string fileName = "dictionaries.csv";
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


        [Route("dictionaries/export/{doc}")]
        public async Task<HttpResponseMessage> PostExport([FromUri]Pagination pagination, string doc = "pdf")
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
            List<object> titles = new List<object>();
            titles.Add(Core.Extensions.GetReportTitles(lan));
            if (!new string[] { "pdf", "xls" }.Contains(doc)) throw new Exception("");// return Task.Run(() => { return BadRequest(); });
            var user = await this.User.Identity.GetUser();
            var guid = Guid.NewGuid();
            var file = HostingEnvironment.MapPath(string.Format("~/Template/PDF/{0}.{1}", guid, doc));
            if (!Directory.Exists(HostingEnvironment.MapPath(string.Format("~/Template/PDF")))) Directory.CreateDirectory(HostingEnvironment.MapPath(string.Format("~/Template/PDF")));
            try
            {
                var _ctx = new DictionaryContext();
                Report rpt = new Report();
                var list = await _ctx.FilterAsync(x => true, x => x.Code, ref pagination);
                // bind data 
                // load report 
                var path = "~/Template/Report/dictionaries.frx";
                rpt.Load(HostingEnvironment.MapPath(path));
                rpt.RegisterData(list.ToList(), "list");
                rpt.RegisterData(titles, "titles");

                // prepare report  
                rpt.Prepare();

                Excel2007Export export_xls = new Excel2007Export();
                PDFExport export_pdf = new PDFExport();

                string mime = "application/pdf";
                switch (doc)
                {
                    case "xls":
                        mime = "application/vnd.ms-excel";
                        export_xls.Export(rpt, file);
                        break;
                    case "pdf":
                    default:
                        mime = "application/pdf";
                        export_pdf.Export(rpt, file);
                        break;
                }
                // return stream in browser 
                HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.OK);
                if (File.Exists(file))
                {
                    using (FileStream reader = File.OpenRead(file))
                    {
                        var buffer = new byte[reader.Length];
                        reader.Read(buffer, 0, buffer.Length);
                        var memory = new MemoryStream(buffer);

                        result.Content = new StreamContent(memory, buffer.Length);
                    }
                    File.Delete(file);
                }
                //result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(mime);
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "dictionary" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + doc;

                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 检索品牌
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("dictionaries/search")]
        [ResponseType(typeof(IDaoData<DictionaryData>))]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var list = ctx.Filter(o => true, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<DictionaryData> { list = list, pagination = pagination });
        }

        #endregion
    }
}