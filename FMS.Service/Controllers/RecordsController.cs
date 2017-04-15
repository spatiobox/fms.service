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
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using FMS.Service.Core.Identity;
using System.Web.Hosting;
using System.IO;
using System.Net.Http.Headers;
using FastReport;
using FastReport.Export.Pdf;
using FastReport.Export.OoXML;
using NPOI;
using NPOI.HPSF;
using NPOI.HSSF;
using NPOI.HSSF.UserModel;
using NPOI.POIFS;
using NPOI.Util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FMS.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class RecordsController : ApiController
    {
        private OmsContext db = new OmsContext();
        private RecordContext ctx = new RecordContext();
        private AuthRepository repo = new AuthRepository();

        // GET: api/Records
        [ResponseType(typeof(IDaoData<RecordData>))]
        public async Task<IDaoData<RecordData>> GetRecords([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var isAdmin = await user.IsAdministrator();
            var list = await ctx.FilterAsync(x => isAdmin || x.UserID == user.Id, x => x.RecordDate, ref pagination);
            return new IDaoData<RecordData>() { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination };
        }


        [ResponseType(typeof(IDaoData<FormularData>))]
        [Route("records/with/formulas")]
        public async Task<IDaoData<FormularData>> GetFormulas([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var ctx_formula = new FormularContext();
            var isAdmin = await user.IsAdministrator();
            var list = await ctx_formula.FilterAsync(x => /* isAdmin || x.UserID == user.Id */ true, ref pagination);
            return new IDaoData<FormularData>() { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination };
        }

        // GET: api/Records/5
        [ResponseType(typeof(RecordData))]
        public async Task<IHttpActionResult> GetRecord(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            Record record = await db.Records.FindAsync(id);
            if (record == null || user.Id != record.UserID)
            {
                return NotFound();
            }

            return Ok(record.ToViewData(suffix));
        }

        // PUT: api/Records/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutRecord(Guid id, RecordData record)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != record.ID)
            {
                return BadRequest();
            }

            var user = await this.User.Identity.GetUser();
            if (user.Id != record.UserID)
            {
                return BadRequest();
            }
            db.Entry(record.ToModel()).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecordExists(id))
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

        // POST: api/Records
        [ResponseType(typeof(RecordData))]
        public async Task<IHttpActionResult> PostRecord(RecordData record)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //db.Records.Add(record.To());

            try
            {
                var user = await this.User.Identity.GetUser();
                record.ID = Guid.NewGuid();
                record.UserID = user.Id;
                record.RecordDate = DateTime.Now;
                var node = await ctx.CreateAsync(record.ToModel());
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (RecordExists(record.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }
        // POST: api/Records
        [ResponseType(typeof(List<RecordData>))]
        [Route("records/batch")]
        public async Task<IHttpActionResult> PostRecord(IList<RecordData> list)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //db.Records.Add(record.To());

            try
            {
                var user = await this.User.Identity.GetUser();
                var records = new List<Record>();
                foreach (var record in list)
                {
                    record.ID = Guid.NewGuid();
                    record.UserID = user.Id;
                    record.RecordDate = DateTime.Now;
                    records.Add(record.ToModel());
                }
                var result = await ctx.CreateAsync(records);
                var models = result.Select(x => x.ToViewData()).ToList();
                return Ok(models);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // DELETE: api/Records/5
        [ResponseType(typeof(RecordData))]
        public async Task<IHttpActionResult> DeleteRecord(Guid id)
        {
            var user = await this.User.Identity.GetUser();
            Record record = await db.Records.FindAsync(id);
            if (record == null || user.Id != record.UserID)
            {
                return NotFound();
            }

            db.Records.Remove(record);
            await db.SaveChangesAsync();

            return Ok(record);
        }


        // DELETE: api/Records/5 
        [ResponseType(typeof(RecordData))]
        [Route("records/batch")]
        public async Task<IHttpActionResult> DeleteRecords(CommonData node)
        {
            var user = await this.User.Identity.GetUser();
            var guids = node.guids;
            var isAdmin = await user.IsAdministrator();
            //var guids = ids.Select(x => Guid.Parse(x)).ToList();
            //var guids =  ids.Split(',').Select(x => new Guid(x)).ToList();
            //var guids = list.Select(x => x.ID).ToList();
            var records = db.Records.Where(x => (isAdmin || x.UserID == user.Id) && guids.Contains(x.ID)).ToList();
            if (records.Count == 0)
            {
                return NotFound();
            }
            if (records.Count != guids.Count)
            {
                var arr = guids.Where(x => !records.Select(r => r.ID).Contains(x)).ToList();
                return BadRequest("Records cannot found: " + string.Join(",", arr));
            }

            db.Records.RemoveRange(records);
            await db.SaveChangesAsync();

            return Ok(guids);
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

        private bool RecordExists(Guid id)
        {
            return db.Records.Count(e => e.ID == id) > 0;
        }

        [Route("records/export/{doc}")]
        public async Task<HttpResponseMessage> PostExport(Pagination pagination, string doc = "pdf")
        {
            if (!new string[] { "pdf", "xls" }.Contains(doc)) throw new Exception("");// return Task.Run(() => { return BadRequest(); });

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
            var guid = Guid.NewGuid();
            var file = HostingEnvironment.MapPath(string.Format("~/Template/PDF/{0}.{1}", guid, doc));
            if (!Directory.Exists(HostingEnvironment.MapPath(string.Format("~/Template/PDF")))) Directory.CreateDirectory(HostingEnvironment.MapPath(string.Format("~/Template/PDF")));
            try
            {
                var _ctx = new RecordContext();
                var isAdmin = await user.IsAdministrator();
                Report rpt = new Report();
                var list = await _ctx.FilterAsync(x => isAdmin || x.UserID == user.Id, x => new { x.FormularCode, x.RecordDate }, ref pagination);
                var data = list.ToList().Select(x => x.ToViewData()).ToList();
                // bind data 
                // load report 
                var path = "~/Template/Report/records.frx";
                rpt.Load(HostingEnvironment.MapPath(path));
                rpt.RegisterData(data, "record");
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
                result.Content.Headers.ContentDisposition.FileName = "record" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + doc;

                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //[Route("records/export/{language}")]
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
        private string ExcelStream(List<RecordData> list, string lang)
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
                    //obj["Directory"].ToString(),
                    obj["FormularTitle"].ToString(),
                    obj["MaterialTitle"].ToString(),
                    obj["Copies"].ToString(),
                    obj["Ratio"].ToString(),
                    obj["RecordDate"].ToString(),
                    //obj["BatchNo"].ToString(),
                    //obj["Viscosity"].ToString(),
                    obj["FullName"].ToString(),
                    obj["Department"].ToString(),
                    obj["Position"].ToString()
                };

                for (int i = 0; i < titles.Length; i++)
                {
                    headerRow.CreateCell(i).SetCellValue(titles[i]);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(i + 1);

                    //dataRow.CreateCell(0).SetCellValue(list[i].OrgTitle);
                    dataRow.CreateCell(0).SetCellValue(list[i].FormularTitle);
                    dataRow.CreateCell(1).SetCellValue(list[i].MaterialTitle);
                    dataRow.CreateCell(2).SetCellValue(list[i].Copies);
                    dataRow.CreateCell(3).SetCellValue(list[i].Weight.ToString());
                    dataRow.CreateCell(4).SetCellValue(list[i].RecordDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    //dataRow.CreateCell(6).SetCellValue(list[i].BatchNo);
                    //dataRow.CreateCell(7).SetCellValue(list[i].Viscosity);
                    dataRow.CreateCell(5).SetCellValue(list[i].FullName);
                    dataRow.CreateCell(6).SetCellValue(list[i].Department);
                    dataRow.CreateCell(7).SetCellValue(list[i].Position);
                }
                workbook.Write(ms);
                ms.Flush();
                var guid = Guid.NewGuid();
                if (!Directory.Exists(HostingEnvironment.MapPath("~/Template/Excel"))) Directory.CreateDirectory(HostingEnvironment.MapPath("~/Template/Excel"));
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
        //private string ExcelStream(List<RecordData> list, string lang)
        //{
        //    Microsoft.Office.Interop.Excel.Application app = null;
        //    Microsoft.Office.Interop.Excel.Workbooks workbooks = null;
        //    Microsoft.Office.Interop.Excel.Workbook wb = null;
        //    Microsoft.Office.Interop.Excel.Worksheet sheet = null;
        //    string file = "";
        //    try
        //    {
        //        app = new Microsoft.Office.Interop.Excel.Application();
        //        workbooks = app.Workbooks;
        //        wb = workbooks.Add(true);
        //        sheet = new Microsoft.Office.Interop.Excel.Worksheet();
        //        //Microsoft.Office.Interop.Excel._Worksheet sheet = (Microsoft.Office.Interop.Excel._Worksheet)sheets.Add(true);
        //        //sheet = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets.get_Item(0);
        //        sheet = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets["sheet1"];
        //        sheet.Name = lang == "zh-cn" ? "配方明细管理" : ("en-us" == lang ? "Formula Details" : "配方明细管理"); ;

        //        var titles_en = new string[] { "Device", "Formular Name", "Material Name", "Standard Weight", "Weight", "Copies", "Date" };
        //        var titles_cn = new string[] { "设备", "配方名称", "配料名称", "标准重", "实际重", "份数", "日期" };
        //        var titles_tw = new string[] { "设备", "配方名称", "配料名称", "标准重", "实际重", "份数", "日期" };
        //        var titles = lang == "zh-cn" ? titles_cn : ("en-us" == lang ? titles_en : titles_tw);
        //        sheet.Cells[1, 1] = titles[0];
        //        sheet.Cells[1, 2] = titles[1];
        //        sheet.Cells[1, 3] = titles[2];
        //        sheet.Cells[1, 4] = titles[3];
        //        sheet.Cells[1, 5] = titles[4];
        //        sheet.Cells[1, 6] = titles[5];
        //        sheet.Cells[1, 7] = titles[6];

        //        //((Microsoft.Office.Interop.Excel.Range)sheet.Rows[11, Missing.Value]).Insert(Missing.Value, Microsoft.Office.Interop.Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
        //        for (int i = 1; i < list.Count; i++)
        //        {
        //            sheet.Cells[i + 1, 1] = list[i - 1].Device;
        //            sheet.Cells[i + 1, 2] = list[i - 1].FormularTitle;
        //            sheet.Cells[i + 1, 3] = list[i - 1].MaterialTitle;
        //            sheet.Cells[i + 1, 4] = list[i - 1].StandardWeight;
        //            sheet.Cells[i + 1, 5] = list[i - 1].Weight;
        //            sheet.Cells[i + 1, 6] = list[i - 1].Copies;
        //            sheet.Cells[i + 1, 7] = list[i - 1].RecordDate;
        //        }
        //        var guid = Guid.NewGuid();
        //        file = string.Format("~/Template/Excel/{0}.xls", guid);
        //        app.ActiveWorkbook.SaveAs(System.Web.Hosting.HostingEnvironment.MapPath(file));
        //        app.DisplayAlerts = false;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //    finally
        //    {
        //        //if (sheet != null) sheet.Delete();
        //        if (wb != null) wb.Close(null, null, null);
        //        if (workbooks != null) workbooks.Close();
        //        if (app != null) app.Quit();
        //        app = null;
        //        GC.Collect();//垃圾回收

        //    }
        //    return file;

        //    //return File(file, "application/vnd.ms-excel", "保税订单.xls");
        //}


        [ResponseType(typeof(IEnumerable<RecordData>))]
        [Route("records/by/{category}/{id}")]
        public async Task<IHttpActionResult> GetByCategory(CategoryDictionary category, Guid id, [FromUri] Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            IEnumerable<Record> list = null;
            var user = await this.User.Identity.GetUser();
            try
            {
                switch (category)
                {
                    case CategoryDictionary.Formular:
                        list = ctx.Filter(x => x.UserID == user.Id && x.FormularID == id, ref pagination);
                        break;
                    case CategoryDictionary.User:
                        list = ctx.Filter(x => x.UserID == user.Id && x.UserID == id.ToString(), ref pagination);
                        break;
                    case CategoryDictionary.Material:
                        list = ctx.Filter(x => x.UserID == user.Id && x.MaterialID == id, ref pagination);
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
            return Ok(list.ToList().Select(x => x.ToViewData(suffix)).OrderBy(x => x.MaterialCode));
        }


        /// <summary>
        /// 检索品牌
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("records/search")]
        [ResponseType(typeof(IDaoData<RecordData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            try
            {

                var user = await this.User.Identity.GetUser();
                var isAdmin = await user.IsAdministrator();
                var list = (await ctx.FilterAsync(x => isAdmin || x.UserID == user.Id, x => new { x.FormularCode, x.RecordDate }, ref pagination)).ToList().Select(x => x.ToViewData(suffix)).ToList();
                return Ok(new IDaoData<RecordData> { list = list, pagination = pagination });
            }
            catch (Exception ex)
            {
                MyConsole.Log(ex, "Records Search");
                throw ex;
            }
        }

    }
}