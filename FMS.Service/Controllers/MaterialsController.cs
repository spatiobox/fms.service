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
using FMS.Service.DAO;
using System.IO;
using FMS.Service.Core.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Web.Hosting;
using FastReport;
using FastReport.Export.Pdf;
using FastReport.Export.OoXML;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json.Linq;

namespace FMS.Service.Controllers
{
    /// <summary>
    /// 原料
    /// </summary>
    [Authorize]
    [RoutePrefix("api")]
    public class MaterialsController : ApiController
    {
        private MaterialContext ctx = new MaterialContext();
        private RecipeContext ctx_recipe = new RecipeContext();
        private AuthRepository repo = new AuthRepository();

        // GET: api/Materials
        [ResponseType(typeof(IDaoData<MaterialData>))]
        public async Task<IHttpActionResult> GetMaterials([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(o => /* o.UserID == user.Id */ true, ref pagination);
            return Ok(new { list = list.OrderBy(x => x.Code).ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Materials/5
        [ResponseType(typeof(MaterialData))]
        public async Task<IHttpActionResult> GetMaterial(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Material material = await ctx.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            return Ok(material.ToViewData(suffix));
        }

        // PUT: api/Materials/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMaterial(Guid id, MaterialData material)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != material.ID)
            {
                return BadRequest();
            }
            var user = await this.User.Identity.GetUser();
            if (user.Id != material.UserID)
            {
                return BadRequest();
            }


            try
            {
                await ctx.UpdateAsync(material.ToModel());
            }
            catch (Exception ex)
            {
                if (!MaterialExists(id))
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

        // POST: api/Materials
        [ResponseType(typeof(MaterialData))]
        public async Task<IHttpActionResult> PostMaterial(MaterialData material)
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
                material.ID = Guid.NewGuid();
                material.UserID = user.Id;
                var node = await ctx.CreateAsync(material.ToModel());

                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (MaterialExists(material.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }
        }

        // DELETE: api/Materials/5
        [ResponseType(typeof(MaterialData))]
        public async Task<IHttpActionResult> DeleteMaterial(Guid id)
        {
            var user = await this.User.Identity.GetUser();
            Material material = await ctx.FindAsync(id);
            if (material == null || material.UserID != user.Id)
            {
                return NotFound();
            }
            List<string> msgs = new List<string>();
            var recipes = ctx.context.Recipes.Where(x => x.UserID == user.Id && x.MaterialID == material.ID).ToList();
            if (recipes.Count > 0)
            {
                msgs.Add(string.Format("如下配方使用了本配料中，请先删除配方详细中的配料：{0}", string.Join(",", recipes.Select(x => "[" + x.Formular.Code + ":" + x.Formular.Title + "]"))));
            }

            //if (material.Records.Count > 0)
            //{
            //    msgs.Add(string.Format("配料被用在一些配方中：{0}", string.Join(",", material.Recipes.Select(x=> "[" +x.Formular.Code + ":" + x.Formular.Title + "]"))));
            //}
            if (msgs.Count > 0)
            {
                return BadRequest(string.Join("|", msgs));
            }

            await ctx.DeleteAsync(material);

            return Ok(material.ToViewData());
        }


        // DELETE: api/Records/5 
        [ResponseType(typeof(MaterialData))]
        [Route("materials/batch")]
        public async Task<IHttpActionResult> DeleteFormulars(CommonData node)
        {
            var user = await this.User.Identity.GetUser();
            var guids = node.guids;
            //var guids = ids.Select(x => Guid.Parse(x)).ToList();
            //var guids =  ids.Split(',').Select(x => new Guid(x)).ToList();
            //var guids = list.Select(x => x.ID).ToList();
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var materials = ctx.context.Materials.Where(x => x.UserID == user.Id && guids.Contains(x.ID)).ToList();
                    if (materials.Count == 0)
                    {
                        return NotFound();
                    }
                    if (materials.Count != guids.Count)
                    {
                        var arr = guids.Where(x => !materials.Select(r => r.ID).Contains(x)).ToList();
                        return BadRequest("Materials cannot found: " + string.Join(",", arr));
                    }
                    var exist = materials.SelectMany(x => x.Recipes);
                    if (exist.Count() > 0)
                    {
                        var msg = "";
                        var ms = materials.Where(x => x.Recipes.Count() > 0).ToList();
                        foreach (var m in ms)
                        {
                            msg += string.Format("|{0}:{1}", m.Title, string.Join(",", m.Recipes.Select(x => x.Formular.Title).Distinct()));
                        }
                        return BadRequest(msg.Substring(1));
                    }

                    ctx.context.Materials.RemoveRange(materials);
                    await ctx.context.SaveChangesAsync();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }

            return Ok(guids);
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

        private bool MaterialExists(Guid id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }


        #region Extra

        /// <summary>
        /// 更新原料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        [ResponseType(typeof(MaterialData))]
        public async Task<IHttpActionResult> PatchMaterial(Guid id, Newtonsoft.Json.Linq.JObject material)
        {
            try
            {
                var user = await this.User.Identity.GetUser();
                var f = await ctx.FindAsync(id);
                if (user.Id != f.UserID)
                {
                    return NotFound();
                }

                var node = ctx.Update(material, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!MaterialExists(id))
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
        /// 根据类型获取原料列表 
        /// </summary>
        /// <param name="category">类型</param>
        /// <param name="id">相应类型的id值</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<MaterialData>))]
        [Route("materials/by/{category:int}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            IEnumerable<Material> list = null;
            var user = await this.User.Identity.GetUser();
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Formular:
                        list = ctx.Filter(x => /* x.UserID == user.Id */ true && x.ID == id).SelectMany(x => x.Recipes).Select(x => x.Material);
                        break;
                    case CategoryDictionary.Recipe:
                        list = ctx_recipe.Filter(x => /* x.UserID == user.Id */ true && x.ID == id).Select(x => x.Material);
                        break;
                    case CategoryDictionary.User:
                        list = ctx.Filter(x => /* x.UserID == user.Id */ true && x.UserID == id.ToString());
                        break;
                    case CategoryDictionary.Material:
                    case CategoryDictionary.Role:
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
        [Route("materials/download/template")]
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
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/materials.{0}.csv", lan.ToLower()));
            string fileName = "materials.csv";
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
        /// 导入数据
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<MaterialData>))]
        [Route("materials/import")]
        public async Task<IHttpActionResult> PostTemplate()
        {
            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin) return BadRequest("invalid_permission");
            // 检查是否是 multipart/form-data
            //if (!Request.Content.IsMimeMultipartContent("multipart/form-data"))
            //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var guid = Guid.NewGuid();
            var path = HostingEnvironment.MapPath(string.Format("/Template/temp/{0}", guid));

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            // 设置上传目录
            var provider = new MultipartFormDataStreamProvider(path);

            var task = await Request.Content.ReadAsMultipartAsync(provider).ContinueWith<IHttpActionResult>((t) =>
            {
                List<Material> materials = new List<Material>();
                int index = 1;
                foreach (var i in provider.FileData)
                {
                    var filename = i.Headers.ContentDisposition.FileName.Trim('"');
                    var info = new FileInfo(i.LocalFileName);
                    List<string> msg = new List<string>();
                    using (var reader = new StreamReader(info.OpenRead()))
                    {
                        string line = reader.ReadLine();
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(line.Trim())) continue;
                            var str = line.Split(',');
                            if (str.Length != 2)
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：{1} ", index, line));
                            }
                            var code = str[0].Trim();
                            var title = str[1].Trim();
                            if (string.IsNullOrEmpty(code))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：编号（Code）不能为空 ", index));
                            }
                            if (string.IsNullOrEmpty(title))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：名称（Name）不能为空 ", index));
                            }
                            var exist_codes_in_doc = materials.Where(x => x.Code == code);
                            var exist_titles_in_doc = materials.Where(x => x.Title == title);
                            if (exist_codes_in_doc.Count() > 0)
                            {
                                msg.Add(string.Format("行{0}内容出错：导入的文档中存在相同编号（Code）[{1}]的记录 ", index, code));
                            }
                            if (exist_titles_in_doc.Count() > 0)
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：导入的文档中存在相同名称（Name）[{1}]的记录 ", index, title));
                            }
                            if (msg.Count == 0)
                            {
                                try
                                {
                                    var node = new Material();
                                    node.ID = Guid.NewGuid();
                                    node.Code = code;
                                    node.Title = title;
                                    node.UserID = user.Id;
                                    materials.Add(node);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                            index++;
                        }
                    }
                    var codes = materials.Select(x => x.Code);
                    var titles = materials.Select(x => x.Title);
                    var exist_codes = ctx.Filter(x => x.UserID == user.Id && codes.Contains(x.Code));
                    var exist_titles = ctx.Filter(x => x.UserID == user.Id && titles.Contains(x.Title));
                    if (exist_codes.Count() > 0)
                    {
                        msg.Add(string.Format("已存在的相同编号：{0} ", string.Join(",", exist_codes.Select(x => x.Code))));
                    }
                    if (exist_titles.Count() > 0)
                    {
                        msg.Add(string.Format("已存在的相同名称：{0} ", string.Join(",", exist_titles.Select(x => x.Title))));
                    }
                    if (index - 1 != materials.Count && msg.Count == 0)
                    {
                        msg.Add("可识别的行数与实际行数不相等，请删除空行 ");
                    }
                    if (msg.Count == 0)
                    {
                        try
                        {
                            var list = ctx.context.Materials.AddRange(materials);
                            ctx.context.SaveChanges();
                            if (Directory.Exists(path)) Directory.Delete(path, true);
                            return Ok(list.Select(x => x.ToViewData()));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    return BadRequest(string.Join("|", msg));
                }
                return BadRequest();
            });
            return task;
        }


        [Route("materials/export/{doc}")]
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
            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin) return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid_permission");
            var guid = Guid.NewGuid();
            var file = HostingEnvironment.MapPath(string.Format("~/Template/PDF/{0}.{1}", guid, doc));
            if (!Directory.Exists(HostingEnvironment.MapPath(string.Format("~/Template/PDF")))) Directory.CreateDirectory(HostingEnvironment.MapPath(string.Format("~/Template/PDF")));
            try
            {
                var _ctx = new MaterialContext();
                Report rpt = new Report();
                var list = await _ctx.FilterAsync(x => x.UserID == user.Id, x => x.Code, ref pagination);
                var data = list.ToList().Select(x => x.ToViewData()).ToList();
                // bind data 
                // load report 
                var path = "~/Template/Report/materials.frx";
                rpt.Load(HostingEnvironment.MapPath(path));
                rpt.RegisterData(data, "material");
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
                    using (FileStream reader = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        var buffer = new byte[reader.Length];
                        reader.Read(buffer, 0, buffer.Length);
                        var memory = new MemoryStream(buffer);

                        result.Content = new StreamContent(memory);
                    }
                    File.Delete(file);
                }
                //result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(mime);
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "material" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + doc;

                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        ////得到excel文件流
        private string ExcelStream(List<MaterialData> list, string lang)
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
                    obj["Code"].ToString(),
                    obj["Title"].ToString()
                };

                for (int i = 0; i < titles.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(titles[i]);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(i + 1);

                    dataRow.CreateCell(0).SetCellValue(list[i].Code);
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
        /// 检索
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("materials/search")]
        [ResponseType(typeof(IDaoData<MaterialData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(x => /* x.UserID == user.Id */ true, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<MaterialData> { list = list, pagination = pagination });
        }

        #endregion
    }
}