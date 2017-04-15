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
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using FMS.Service.Core.Identity;
using System.Web.Hosting;
using System.Net.Http.Headers;
using FastReport;
using FastReport.Export.Pdf;
using FastReport.Export.OoXML;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json.Linq;

namespace FMS.Service.Controllers
{
    /// <summary>
    /// 配方明细
    /// </summary>
    [Authorize]
    [RoutePrefix("api")]
    public class RecipesController : ApiController
    {
        private RecipeContext ctx = new RecipeContext();
        private FormularContext ctx_formular = null;
        private MaterialContext ctx_material = null;
        private AuthRepository repo = new AuthRepository();

        public RecipesController()
        {
            ctx_formular = new FormularContext(ctx.context);
            ctx_material = new MaterialContext(ctx.context);
        }

        // GET: api/Recipes
        [ResponseType(typeof(IDaoData<RecipeData>))]
        public async Task<IHttpActionResult> GetRecipes([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = await ctx.FilterAsync(x => /* x.UserID == user.Id */ true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Recipes/5
        [ResponseType(typeof(RecipeData))]
        public async Task<IHttpActionResult> GetRecipe(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Recipe recipe = await ctx.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return Ok(recipe.ToViewData(suffix));
        }

        // PUT: api/Recipes/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutRecipe(Guid id, RecipeData recipe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != recipe.ID)
            {
                return BadRequest();
            }
            var user = await this.User.Identity.GetUser();
            if (user.Id != recipe.UserID)
            {
                return BadRequest();
            }


            try
            {
                await ctx.UpdateAsync(recipe.ToModel());
            }
            catch (Exception ex)
            {
                if (!RecipeExists(id))
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

        // POST: api/Recipes
        [ResponseType(typeof(RecipeData))]
        public async Task<IHttpActionResult> PostRecipe(RecipeData recipe)
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
                recipe.ID = Guid.NewGuid();
                recipe.UserID = user.Id;
                var node = await ctx.CreateAsync(recipe.ToModel());
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (RecipeExists(recipe.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }

        // DELETE: api/Recipes/5
        [ResponseType(typeof(RecipeData))]
        public async Task<IHttpActionResult> DeleteRecipe(Guid id)
        {
            Recipe recipe = await ctx.FindAsync(id);
            var user = await this.User.Identity.GetUser();
            if (recipe == null || recipe.UserID != user.Id)
            {
                return NotFound();
            }

            await ctx.DeleteAsync(recipe);

            return Ok(recipe);
        }


        // DELETE: api/Records/5 
        [ResponseType(typeof(RecipeData))]
        [Route("recipes/batch")]
        public async Task<IHttpActionResult> DeleteRecipes(CommonData node)
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
                    var recipes = ctx.context.Recipes.Where(x => x.UserID == user.Id && guids.Contains(x.ID)).ToList();
                    if (recipes.Count == 0)
                    {
                        return NotFound();
                    }
                    if (recipes.Count != guids.Count)
                    {
                        var arr = guids.Where(x => !recipes.Select(r => r.ID).Contains(x)).ToList();
                        return BadRequest("Recipes cannot found: " + string.Join(",", arr));
                    }

                    ctx.context.Recipes.RemoveRange(recipes);
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
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RecipeExists(Guid id)
        {
            return ctx.Count(e => e.ID == id) > 0;
        }


        #region Extra

        /// <summary>
        /// 更新配方明细
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recipe"></param>
        /// <returns></returns>
        [ResponseType(typeof(RecipeData))]
        public async Task<IHttpActionResult> PatchRecipe(Guid id, Newtonsoft.Json.Linq.JObject recipe)
        {
            try
            {
                var user = await this.User.Identity.GetUser();
                var r = await ctx.FindAsync(id);
                if (user.Id != r.UserID)
                {
                    return NotFound();
                }

                var node = ctx.Update(recipe, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!RecipeExists(id))
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
        /// 根据类型获取配方明细列表 
        /// </summary>
        /// <param name="category">类型</param>
        /// <param name="id">相应类型的id值</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<RecipeData>))]
        [Route("recipes/by/{category}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, Guid id, [FromUri] Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            IEnumerable<Recipe> list = null;
            var user = await this.User.Identity.GetUser();
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Formular:
                        list = ctx.Filter(x => /* x.UserID == user.Id */ true && x.FormularID == id, ref pagination);
                        break;
                    case CategoryDictionary.User:
                        list = ctx.Filter(x => /* x.UserID == user.Id */ true && x.UserID == id.ToString(), ref pagination);
                        break;
                    case CategoryDictionary.Material:
                        list = ctx.Filter(x => /* x.UserID == user.Id */ true && x.MaterialID == id, ref pagination);
                        break;
                    case CategoryDictionary.Role:
                    case CategoryDictionary.Permission:
                    case CategoryDictionary.Recipe:
                    default:
                        return BadRequest("系统不支持此功能");
                        break;
                }
            }
            catch
            {
                throw;
            }
            return Ok(list.ToList().Select(x => x.ToViewData(suffix)));
        }


        /// <summary>
        /// 下载模板
        /// </summary>
        /// <returns>Excel文档</returns>
        [Route("recipes/download/template")]
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
            string file = HostingEnvironment.MapPath(string.Format("~/Template/CSV/recipes.{0}.csv", lan.ToLower()));
            string fileName = "recipes.csv";
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
        [ResponseType(typeof(IList<RecipeData>))]
        [Route("recipes/import")]
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
                List<Recipe> recipes = new List<Recipe>();
                int index = 1;
                foreach (var i in provider.FileData)
                {
                    var formulars = ctx_formular.Filter(x => x.UserID == user.Id).ToList();
                    var materials = ctx_material.Filter(x => x.UserID == user.Id).ToList();
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
                            if (str.Length != 6)
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：{1} ", index, line));
                            }
                            var formularcode = str[0].Trim();
                            var formulartitle = str[1].Trim();
                            var materialcode = str[2].Trim();
                            var materialtitle = str[3].Trim();
                            decimal weight = 0;
                            if (!decimal.TryParse(str[4], out weight))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：标准重量的值应该是数值类型 ", index));
                            }
                            decimal deviation = 0;
                            if (!decimal.TryParse(str[5], out deviation))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：误差量的值应该是数值类型 ", index));
                            }

                            if (string.IsNullOrEmpty(formulartitle))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：配方名称不能为空 ", index));
                            }
                            if (string.IsNullOrEmpty(formularcode))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：配方编码不能为空 ", index));
                            }
                            if (string.IsNullOrEmpty(materialtitle))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：配料名称不能为空 ", index));
                            }
                            if (string.IsNullOrEmpty(materialcode))
                            {
                                msg.Add(string.Format("行{0}内容出错，请检查：配料编码不能为空 ", index));
                            }

                            var formular = formulars.FirstOrDefault(x => x.Code == formularcode && x.Title == formulartitle);
                            if (formular == null)
                            {
                                if (formulars.Any(x => x.Code == formularcode || x.Title == formulartitle))
                                {
                                    msg.Add(string.Format("行{0}内容出错，请检查：配方名称与配方编码不匹配 ", index));
                                }
                                else
                                {
                                    formular = new Formular()
                                    {
                                        ID = Guid.NewGuid(),
                                        Title = formulartitle,
                                        Code = formularcode,
                                        CreateDate = DateTime.Now,
                                        UserID = user.Id
                                    };
                                    ctx_formular.Create(formular);
                                    formulars.Add(formular);
                                }
                            }

                            var material = materials.FirstOrDefault(x => x.Code == materialcode && x.Title == materialtitle);
                            if (material == null)
                            {
                                if (formulars.Any(x => x.Code == materialcode || x.Title == materialtitle))
                                {
                                    msg.Add(string.Format("行{0}内容出错，请检查：配料名称与配料编码不匹配 ", index));
                                }
                                else
                                {
                                    material = new Material()
                                    {
                                        ID = Guid.NewGuid(),
                                        Title = materialtitle,
                                        Code = materialcode,
                                        UserID = user.Id
                                    };
                                    ctx_material.Create(material);
                                    materials.Add(material);
                                }
                            }

                            var exist_in_doc = recipes.Where(x => x.FormularID == formular.ID && x.MaterialID == material.ID);
                            if (exist_in_doc.Count() > 0)
                            {
                                msg.Add(string.Format("行{0}内容出错：导入的文档中同一配方[{1}]存在相同配料[{2}]的记录 ", index, formular.Title, material.Title));
                            }
                            if (msg.Count == 0)
                            {
                                try
                                {
                                    var node = new Recipe()
                                    {
                                        ID = Guid.NewGuid(),
                                        FormularID = formular.ID,
                                        MaterialID = material.ID,
                                        Sort = 0,
                                        Deviation = deviation,
                                        IsRatio = false,
                                        Weight = weight,
                                        UserID = user.Id
                                    };
                                    recipes.Add(node);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                            index++;
                        }
                    }
                    var exist_in_database = ctx.Filter(x => x.UserID == user.Id).ToList().Join(recipes, db => new { db.FormularID, db.MaterialID }, current => new { current.FormularID, current.MaterialID }, (db, current) => new { db, current });
                    if (exist_in_database.Count() > 0)
                    {
                        string str = "已存在的相同配方详细： ";
                        foreach (var item in exist_in_database)
                        {
                            str += string.Format("[配方 {0} ， 配料 {1}]", item.db.Formular.Title, item.db.Material.Title);
                        }
                        msg.Add(str);
                    }
                    if (index - 1 != recipes.Count && msg.Count == 0)
                    {
                        msg.Add("可识别的行数与实际行数不相等，请删除空行 ");
                    }
                    if (msg.Count == 0)
                    {
                        try
                        {
                            var list = ctx.context.Recipes.AddRange(recipes);
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


        [Route("recipes/export/{doc}")]
        public async Task<HttpResponseMessage> PostExport(CommonData node, [FromUri]Pagination pagination, string doc = "pdf")
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
            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            if (!isAdmin) return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid_permission");
            var guid = Guid.NewGuid();
            var file = HostingEnvironment.MapPath(string.Format("~/Template/PDF/{0}.{1}", guid, doc));
            if (!Directory.Exists(HostingEnvironment.MapPath(string.Format("~/Template/PDF")))) Directory.CreateDirectory(HostingEnvironment.MapPath(string.Format("~/Template/PDF")));
            try
            {
                var _ctx = new RecipeContext();
                Report rpt = new Report();
                //var list = await _ctx.FilterAsync(x => x.UserID == user.Id, ref pagination);

                IQueryable<Recipe> list;
                if (node != null && node.guids != null && node.guids.Count() > 0)
                {
                    list = await _ctx.FilterAsync(x => node.guids.Contains(x.FormularID), x => x.Formular.Code, ref pagination);
                }
                else
                {
                    list = await _ctx.FilterAsync(x => /* x.UserID == user.Id */ true, x => x.Formular.Code, ref pagination);
                }

                var data = list.ToList().Select(x => x.ToViewData()).ToList();
                // bind data 
                // load report 
                var path = "~/Template/Report/recipes.frx";
                rpt.Load(HostingEnvironment.MapPath(path));
                rpt.RegisterData(data, "recipe");
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

                        result.Content = new StreamContent(memory);
                    }
                    File.Delete(file);
                }
                //result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(mime);
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "recipe" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + doc;

                return result;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //[Route("recipes/export/{language}")]
        //public async Task<HttpResponseMessage> GetExport([FromUri]Pagination pagination, string language = "en-US")
        //{
        //    var user = await this.User.Identity.GetUser();
        //    var list = await ctx.FilterAsync(x => x.UserID == user.Id, ref pagination);
        //    var rs = list.Select(x => x.ToViewData()).ToList();
        //    string path = ExcelStream(rs, language.ToLower());

        //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

        //    if (File.Exists(HostingEnvironment.MapPath(path)))
        //    {
        //        using (FileStream reader = File.OpenRead(HostingEnvironment.MapPath(path)))
        //        {
        //            var buffer = new byte[reader.Length];
        //            reader.Read(buffer, 0, buffer.Length);
        //            var memory = new MemoryStream(buffer);

        //            result.Content = new StreamContent(memory);
        //        }
        //        File.Delete(HostingEnvironment.MapPath(path));
        //    }

        //    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
        //    //we used attachment to force download
        //    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //    result.Content.Headers.ContentDisposition.FileName = "file.xls";
        //    return result;
        //}
        ////得到excel文件流
        private string ExcelStream(List<RecipeData> list, string lang)
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
                    obj["FormularTitle"].ToString(),
                    obj["MaterialTitle"].ToString(),
                    obj["Ratio"].ToString(),
                    obj["DeviationWeight"].ToString()
                };

                for (int i = 0; i < titles.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(titles[i]);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(i + 1);

                    dataRow.CreateCell(0).SetCellValue(list[i].FormularTitle);
                    dataRow.CreateCell(1).SetCellValue(list[i].MaterialTitle);
                    dataRow.CreateCell(2).SetCellValue(list[i].Weight.ToString());
                    dataRow.CreateCell(3).SetCellValue(list[i].DeviationWeight.ToString("f4"));
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
        [Route("recipes/search")]
        [ResponseType(typeof(IDaoData<RecipeData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(x => /* x.UserID == user.Id */ true, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<RecipeData> { list = list, pagination = pagination });
        }

        #endregion
    }
}