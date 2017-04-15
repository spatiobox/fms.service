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
using Newtonsoft.Json.Linq;

namespace FMS.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class BucketsController : ApiController
    {
        private BucketContext ctx = new BucketContext();
        // GET: api/Buckets
        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [ResponseType(typeof(IDaoData<BucketData>))]
        public async Task<IHttpActionResult> GetBuckets([FromUri]Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var list = await ctx.FilterAsync(x => true, ref pagination);
            return Ok(new { list = list.ToList().Select(x => x.ToViewData(suffix)), pagination = pagination });
        }

        // GET: api/Buckets/5
        [ResponseType(typeof(BucketData))]
        public async Task<IHttpActionResult> GetBucket(Guid id, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            Bucket dictionary = await ctx.FindAsync(id);
            if (dictionary == null)
            {
                return NotFound();
            }

            return Ok(dictionary.ToViewData(suffix));
        }

        [Route("buckets/signature/{type}/{scale}")]
        public async Task<IHttpActionResult> GetSignature(string type, string scale, [FromUri]string path = null, [FromUri]Pagination pagination = null, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(scale))
            {
                return BadRequest();
            }
            var list = await ctx.FilterAsync(x => x.Scale == scale, ref pagination);
            var model = list.FirstOrDefault();
            if (model != null)
            {
                var node = model.ToViewData(suffix | CategoryDictionary.Signature);
                node.AppID = MyConsole.Cipher.AppID;

                if (type == "update" || type == "delete" || type == "rm")
                {
                    node.Signature = Sign.SignatureOnce(Convert.ToInt32(node.AppID), MyConsole.Cipher.SecretID, MyConsole.Cipher.SecretKey, path ?? "", node.Title);
                }
                else
                    node.Signature = Sign.Signature(Convert.ToInt32(node.AppID), MyConsole.Cipher.SecretID, MyConsole.Cipher.SecretKey, DateTime.Now.AddMinutes(10).ToUnixTime() / 1000, node.Title);
                return Ok(node);
            }
            return NotFound();
        }

        // PUT: api/Buckets/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBucket(Guid id, BucketData node)
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
                if (!BucketExists(id))
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

        // POST: api/Buckets
        [ResponseType(typeof(BucketData))]
        public async Task<IHttpActionResult> PostBucket(BucketData node)
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
                if (BucketExists(node.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }

        }

        // DELETE: api/Buckets/5
        [ResponseType(typeof(Bucket))]
        public async Task<IHttpActionResult> DeleteBucket(Guid id)
        {
            var user = await this.User.Identity.GetUser();
            Bucket dictionary = await ctx.FindAsync(id);

            List<string> msgs = new List<string>();


            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    ctx.context.Buckets.Remove(dictionary);
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
        [Route("buckets/batch")]
        public async Task<IHttpActionResult> DeleteBuckets(CommonData node)
        {
            var user = await this.User.Identity.GetUser();
            var ids = node.guids;
            using (var tran = ctx.context.Database.BeginTransaction())
            {
                try
                {
                    var buckets = ctx.Filter(x => ids.Contains(x.ID)).ToList();
                    if (buckets.Count == 0)
                    {
                        return NotFound();
                    }
                    if (buckets.Count != ids.Count)
                    {
                        var arr = ids.Where(x => !buckets.Select(r => r.ID).Contains(x)).ToList();
                        return BadRequest("Buckets cannot found: " + string.Join(",", arr));
                    }

                    ctx.context.Buckets.RemoveRange(buckets);
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

        private bool BucketExists(Guid id)
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
        [ResponseType(typeof(BucketData))]
        public IHttpActionResult PatchBucket(Guid id, Newtonsoft.Json.Linq.JObject dictionary)
        {
            try
            {
                var node = ctx.Update(dictionary, id);
                return Ok(node.ToViewData());
            }
            catch (Exception ex)
            {
                if (!BucketExists(id))
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
        /// 检索品牌
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        [Route("buckets/search")]
        [ResponseType(typeof(IDaoData<BucketData>))]
        public async Task<IHttpActionResult> PostSearch(Pagination pagination, [FromUri]CategoryDictionary suffix = CategoryDictionary.None)
        {
            var user = await this.User.Identity.GetUser();
            var list = ctx.Filter(o => true, ref pagination).ToList().Select(x => x.ToViewData(suffix)).ToList();
            return Ok(new IDaoData<BucketData> { list = list, pagination = pagination });
        }

        #endregion


        [ResponseType(typeof(CipherData))]
        [Route("buckets/cipher")]
        public async Task<IHttpActionResult> GetCipher()
        {
            try
            {
                return Ok(MyConsole.Cipher);
            }
            catch (Exception ex)
            {
                MyConsole.Log(ex, "获取Cipher异常");
                throw ex;
            }
        }

        [Route("buckets/cipher")]
        public async Task<IHttpActionResult> PostCipher(CipherData node)
        {
            try
            {
                MyConsole.SaveConfig(node);
                return Ok();
            }
            catch (Exception ex)
            {
                MyConsole.Log(ex, "保存Cipher异常");
                throw ex;
            }

        }
    }
}