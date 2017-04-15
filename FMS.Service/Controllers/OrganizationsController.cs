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
    /// 目录
    /// </summary>
    [RoutePrefix("api")]
    [Authorize]
    public class OrganizationsController : ApiController
    {
        private OrganizationContext ctx = new OrganizationContext();
        private RecipeContext ctx_recipe = new RecipeContext();
        private AuthRepository repo = new AuthRepository();
        // GET: api/Organizations
        /// <summary>
        /// 获取所有目录
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<OrganizationData>))]
        public async Task<IHttpActionResult> GetOrganizations([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => /* x.UserID == user.Id */ true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Organizations/5
        [ResponseType(typeof(OrganizationData))]
        public async Task<IHttpActionResult> GetOrganization(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Organization Organization = await ctx.FindAsync(id);
            if (Organization == null)
            {
                return NotFound();
            }

            return Ok(Organization.ToViewData(suffix));
        }

        // PUT: api/Organizations/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutOrganization(int id, OrganizationData Organization)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != Organization.ID)
            {
                return BadRequest();
            }

            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin)
            {
                return BadRequest();
            }

            try
            {
                await ctx.UpdateAsync(Organization.ToModel());
            }
            catch (Exception ex)
            {
                if (!OrganizationExists(id))
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

        // POST: api/Organizations
        [ResponseType(typeof(OrganizationData))]
        public async Task<IHttpActionResult> PostOrganization(OrganizationData Organization)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin)
            {
                return BadRequest("invalid_permission");
            }

            try
            {
                var node = await ctx.CreateAsync(Organization.ToModel());
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (OrganizationExists(Organization.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }

        // DELETE: api/Organizations/5
        [ResponseType(typeof(Organization))]
        public async Task<IHttpActionResult> DeleteOrganization(int id)
        {
            var user = await this.User.Identity.GetUser();
            Organization Organization = await ctx.FindAsync(id);
            if (Organization == null)
            {
                return NotFound();
            }

            List<string> msgs = new List<string>();
            //if (Organization.Recipes.Count > 0)
            //{
            //    msgs.Add("该目录据有相关明细，请先删除相应的目录明细");
            //}

            //if (msgs.Count > 0)
            //{
            //    return BadRequest(string.Join("|", msgs));
            //}

            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                { 
                    ctx.context.Organizations.Remove(Organization);
                    ctx.context.SaveChanges();
                    //await ctx.DeleteAsync(Organization);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    //var msgs = new List<string>();
                    //if (Organization.Recipes.Count > 0)
                    //{
                    //    msgs.Add("该目录具有{0}")
                    //}
                    tran.Rollback();
                    return BadRequest(ex.Message);
                    //throw ex;
                }
            }

            return Ok(Organization.ToViewData());
        }


        // DELETE: api/Records/5 
        [ResponseType(typeof(OrganizationData))]
        [Route("organizations/batch")]
        public async Task<IHttpActionResult> DeleteOrganizations(CommonData node)
        {
            var user = await this.User.Identity.GetUser();
            var ids = node.ids;
            //var guids = ids.Select(x => Guid.Parse(x)).ToList();
            //var guids =  ids.Split(',').Select(x => new Guid(x)).ToList();
            //var guids = list.Select(x => x.ID).ToList();
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var Organizations = ctx.context.Organizations.Where(x => ids.Contains(x.ID)).ToList();
                    if (Organizations.Count == 0)
                    {
                        return NotFound();
                    }
                    if (Organizations.Count != ids.Count)
                    {
                        var arr = ids.Where(x => !Organizations.Select(r => r.ID).Contains(x)).ToList();
                        return BadRequest("Organizations cannot found: " + string.Join(",", arr));
                    }
                     
                    ctx.context.Organizations.RemoveRange(Organizations);
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
                repo.Dispose();
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrganizationExists(int id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }



        #region Extra

        /// <summary>
        /// 更新目录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Organization"></param>
        /// <returns></returns>
        [ResponseType(typeof(OrganizationData))]
        public async Task<IHttpActionResult> PatchOrganization(int id, Newtonsoft.Json.Linq.JObject Organization)
        {
            try
            {
                var user = await this.User.Identity.GetUser();
                var f = await ctx.FindAsync(id);
                if (f == null)
                {
                    return NotFound();
                }

                var node = ctx.Update(Organization, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!OrganizationExists(id))
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
        /// 根据类型获取目录列表 
        /// </summary>
        /// <param name="category">类型</param>
        /// <param name="id">相应类型的id值</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<OrganizationData>))]
        [Route("organizations/by/{category:int}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, Guid id, Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            IEnumerable<Organization> list = null;
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Recipe:
                        list = ctx.Filter(x => x.Formulas.SelectMany(f=>f.Recipes).Any(rc => rc.ID == id));
                        break;
                    case CategoryDictionary.User:
                        list = ctx.Filter(x => x.Users.Select(u=>u.Id).Contains(id.ToString()));
                        break;
                    case CategoryDictionary.Material:
                    case CategoryDictionary.Role:
                    case CategoryDictionary.Permission:
                    case CategoryDictionary.Organization:
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
        [Route("Organizations/download/template")]
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
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/Organizations.{0}.csv", lan.ToLower()));
            string fileName = "Organizations.csv";
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
        ///// <summary>
        ///// 导入数据
        ///// </summary>
        ///// <returns></returns>
        //[ResponseType(typeof(IList<OrganizationData>))]
        //[Route("Organizations/import")]
        //public async Task<IHttpActionResult> PostTemplate()
        //{
        //    var user = await this.User.Identity.GetUser();
        //    var isAdmin = await user.IsAdministrator();
        //    if (!isAdmin) return BadRequest("invalid_permission");
        //    // 检查是否是 multipart/form-data
        //    //if (!Request.Content.IsMimeMultipartContent("multipart/form-data"))
        //    //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

        //    var guid = Guid.NewGuid();
        //    var path = HostingEnvironment.MapPath(string.Format("/Template/temp/{0}", guid));

        //    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        //    // 设置上传目录
        //    var provider = new MultipartFormDataStreamProvider(path);

        //    var task = await Request.Content.ReadAsMultipartAsync(provider).ContinueWith<IHttpActionResult>((t) =>
        //    {

        //        //if (t.IsFaulted || t.IsCanceled)
        //        //{
        //        //    throw new HttpResponseException(HttpStatusCode.InternalServerError);
        //        //}

        //        //var fileInfo = new List<FileDesc>();
        //        List<Organization> Organizations = new List<Organization>();
        //        int index = 1;
        //        foreach (var i in provider.FileData)
        //        {
        //            var filename = i.Headers.ContentDisposition.FileName.Trim('"');
        //            var info = new FileInfo(i.LocalFileName);
        //            List<string> msg = new List<string>();
        //            using (var reader = new StreamReader(info.OpenRead()))
        //            {
        //                string line = reader.ReadLine();
        //                while ((line = reader.ReadLine()) != null)
        //                {
        //                    if (string.IsNullOrEmpty(line.Trim())) continue;
        //                    var str = line.Split(',');
        //                    if (str.Length != 2)
        //                    {
        //                        msg.Add(string.Format("行{0}内容出错，请检查：{1} ", index, line));
        //                    }
        //                    var code = str[0].Trim();
        //                    var title = str[1].Trim();
        //                    if (string.IsNullOrEmpty(code))
        //                    {
        //                        msg.Add(string.Format("行{0}内容出错，请检查：编号（Code）不能为空 ", index));
        //                    }
        //                    if (string.IsNullOrEmpty(title))
        //                    {
        //                        msg.Add(string.Format("行{0}内容出错，请检查：名称（Name）不能为空 ", index));
        //                    }
        //                    var exist_codes_in_doc = Organizations.Where(x => x.Code == code);
        //                    var exist_titles_in_doc = Organizations.Where(x => x.Title == title);
        //                    if (exist_codes_in_doc.Count() > 0)
        //                    {
        //                        msg.Add(string.Format("行{0}内容出错，请检查：导入的文档中存在相同编号（Code）[{1}]的记录 ", index, code));
        //                    }
        //                    if (exist_titles_in_doc.Count() > 0)
        //                    {
        //                        msg.Add(string.Format("行{0}内容出错，请检查：导入的文档中存在相同名称（Name）[{1}]的记录 ", index, title));
        //                    }
        //                    if (msg.Count == 0)
        //                    {
        //                        try
        //                        {
        //                            var node = new Organization();
        //                            node.Name = code;
        //                            node.Title = title;
        //                            node.UserID = user.Id;
        //                            node.CreateDate = DateTime.Now;
        //                            Organizations.Add(node);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                        }
        //                    }
        //                    index++;
        //                }
        //            }
        //            var codes = Organizations.Select(x => x.Code);
        //            var titles = Organizations.Select(x => x.Title);
        //            var exist_codes = ctx.Filter(x => /* x.UserID == user.Id */ true && codes.Contains(x.Code));
        //            var exist_titles = ctx.Filter(x => /* x.UserID == user.Id */ true && titles.Contains(x.Title));
        //            if (exist_codes.Count() > 0)
        //            {
        //                msg.Add(string.Format("已存在的相同编号的目录：{0} ", string.Join(",", exist_codes.Select(x => x.Code))));
        //            }
        //            if (exist_titles.Count() > 0)
        //            {
        //                msg.Add(string.Format("已存在的相同名称的目录：{0} ", string.Join(",", exist_titles.Select(x => x.Title))));
        //            }
        //            if (index - 1 != Organizations.Count && msg.Count == 0)
        //            {
        //                msg.Add("可识别的行数与实际行数不相等，请删除空行 ");
        //            }
        //            if (msg.Count == 0)
        //            {
        //                try
        //                {
        //                    var list = ctx.context.Organizations.AddRange(Organizations);
        //                    ctx.context.SaveChanges();
        //                    if (Directory.Exists(path)) Directory.Delete(path, true);
        //                    return Ok(list.Select(x => x.ToViewData()));
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw ex;
        //                }
        //            }
        //            if (Directory.Exists(path)) Directory.Delete(path, true);
        //            return BadRequest(string.Join("|", msg));
        //            //info.CopyTo(Path.Combine(path, filename), true);
        //            //fileInfo.Add(new FileDesc(filename, Path.Combine(path, filename), info.Length / 1024));

        //        }

        //        //var fileInfo = provider.FileData.Select(i =>
        //        //{
        //        //    var filename = i.Headers.ContentDisposition.FileName.Trim('"');
        //        //    var info = new FileInfo(i.LocalFileName);
        //        //    info.CopyTo(Path.Combine(path, filename), true);
        //        //    info.Delete();
        //        //    return new FileDesc(filename, Path.Combine(path, filename), info.Length / 1024);
        //        //});

        //        return BadRequest();
        //    });
        //    return task;
        //}


        [Route("organizations/export/{doc}")]
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
            if (acceptedLanguages.Any(x => x.Value == "zh-CN"))
            {
                lan = "zh-CN";
            }
            List<object> titles = new List<object>();
            titles.Add(Core.Extensions.GetReportTitles(lan));
            if (!new string[] { "pdf", "xls" }.Contains(doc)) throw new Exception("");// return Task.Run(() => { return BadRequest(); });
            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin) return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid_permission");
            var guid = Guid.NewGuid();
            var file = HostingEnvironment.MapPath(string.Format("~/Template/PDF/{0}.{1}", guid, doc));
            if (!Directory.Exists(HostingEnvironment.MapPath(string.Format("~/Template/PDF")))) Directory.CreateDirectory(HostingEnvironment.MapPath(string.Format("~/Template/PDF")));
            try
            {
                var _ctx = new OrganizationContext();
                Report rpt = new Report();
                var list = await _ctx.FilterAsync(x =>  true, x => x.ParentID, ref pagination);
                var data = list.ToList().Select(x => x.ToViewData()).ToList();
                // bind data 
                // load report 
                var path = "~/Template/Report/Organizations.frx";
                rpt.Load(HostingEnvironment.MapPath(path));
                rpt.RegisterData(data, "list");
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
                        //export_xls.Export(rpt, file);
                        file = ExcelStream(data, lan);
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
                result.Content.Headers.ContentDisposition.FileName = "Organization" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + doc;

                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        ////得到excel文件流
        private string ExcelStream(List<OrganizationData> list, string lang)
        {
            string file = "";
            MemoryStream ms = new MemoryStream();
            HSSFWorkbook workbook = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)workbook.CreateSheet();
            HSSFRow headerRow = (HSSFRow)sheet.CreateRow(0);
            try
            {
                var ts = Core.Extensions.GetReportTitles(lang);
                //JObject obj = JObject.Parse(ts);
                JObject obj = JObject.FromObject(ts);

                var titles = new string[] {
                    obj["ID"].ToString(),
                    obj["Code"].ToString()
                };

                for (int i = 0; i < titles.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(titles[i]);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(i + 1);

                    dataRow.CreateCell(0).SetCellValue(list[i].ID);
                    dataRow.CreateCell(1).SetCellValue(list[i].Title); 
                }
                workbook.Write(ms);
                ms.Flush();
                var guid = Guid.NewGuid();
                file = System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/Template/Excel/{0}.xls", guid));
                using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
                {
                    BinaryWriter w = new BinaryWriter(fs);
                    w.Write(ms.ToArray());
                    ms.Close();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                sheet = null;
                headerRow = null;
                workbook = null;
                GC.Collect();//垃圾回收 
            }

            return file;
        }

        /// <summary>
        /// 检索品牌
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("organizations/search")]
        [ResponseType(typeof(IDaoData<OrganizationData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(o => 1 == 1, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<OrganizationData> { list = list, pagination = pagination });
        }

        #endregion
    }
}