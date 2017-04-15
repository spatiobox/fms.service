using Microsoft.AspNet.Identity;
using FMS.Service.Core.Identity;
using FMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using FMS.Service.Core;
using System.Net.Http.Headers;
using System.Web;
using System.Text.RegularExpressions;

namespace FMS.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class IdentityController : ApiController
    {
        private AuthRepository _repo = null;
        private UserContext _ctx = null;

        public IdentityController()
        {
            _ctx = new UserContext();
            _repo = new AuthRepository();
        }

        [AllowAnonymous]
        [Route("identity/register")]
        public async Task<IHttpActionResult> Register(UserModel model)
        {
            var list = _ctx.Filter(x => x.UserName == model.UserName || x.Email == model.Email || x.PhoneNumber == model.PhoneNumber).ToList();
            if (list.Count > 0)
            {
                var err = new List<string>();
                if (list.Count(x => x.UserName == model.UserName) > 0)
                {
                    err.Add("register_exist_username");
                }
                if (list.Count(x => x.Email == model.Email) > 0)
                {
                    err.Add("register_exist_email");
                }
                if (list.Count(x => x.PhoneNumber == model.PhoneNumber) > 0)
                {
                    err.Add("register_exist_phone");
                }
                if (err.Count > 0) return BadRequest(string.Join(",", err));
            }
            IdentityResult result = await _repo.RegisterUser(model);
            IHttpActionResult error = GetErrorResult(result);
            if (error != null)
            {
                return error;
            }
            else
            {
                var user = await _repo.FindUserByName(model.UserName);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {

                    HttpHeaderValueCollection<StringWithQualityHeaderValue> acceptedLanguages = Request.Headers.AcceptLanguage;
                    var lan = "zh-CN";
                    var subject = "注册成功";
                    if (acceptedLanguages.Any(x => x.Value == "en-US"))
                    {
                        lan = "en-US";
                        subject = "Register Success";
                    }
                    if (acceptedLanguages.Any(x => x.Value == "zh-TW"))
                    {
                        lan = "zh-TW";
                        subject = "註冊成功";
                    }
                    var ctx = new OmsContext();
                    var profile = new Profile()
                    {
                        UserID = user.Id,
                        Language = lan
                    };
                    ctx.Profiles.Add(profile);
                    ctx.SaveChanges();

                    var temp = "/Template/Mail/register_success_{0}.html";
                    var file = string.Format(temp, lan);
                    var domain = "www.omniteaching.com";
                    try
                    {
                        domain = MyConsole.GetAppString("domain");
                    }
                    catch (Exception)
                    {
                    }
                    var body = MyConsole.ReadFile(file);
                    EmailHelper helper = new EmailHelper(user.Email, subject, body.Replace("{domain}", domain));
                    helper.Send();

                }
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("identity/forgot")]
        public async Task<IHttpActionResult> PostForgot(ForgotModel model)
        {

            try
            {
                ApplicationUser user = await _repo.FindUserByEmail(model.Email);
                if (user == null)
                {
                    return BadRequest();
                }
                else
                {
                    HttpHeaderValueCollection<StringWithQualityHeaderValue> acceptedLanguages = Request.Headers.AcceptLanguage;
                    var token = await _repo.GeneratePasswordResetToken(user.Id);
                    var lan = "zh-CN";
                    var subject = "密码报失";
                    var domain = "www.omniteaching.com";
                    try
                    {
                        domain = MyConsole.GetAppString("domain");
                    }
                    catch (Exception)
                    {
                    }
                    //                    var body = @"
                    //                        请使用<a href='http://product.omniteaching.com/forgot?uid={1}&token={0}'>
                    //                            变更密码
                    //                        </a>
                    //                        或进入以下地址 http://product.omniteaching.com/forgot?uid={1}&token={0}
                    //                        来更换您在 FMS-E APP 上的密码";
                    if (acceptedLanguages.Any(x => x.Value == "en-US"))
                    {
                        lan = "en-US";
                        subject = "Forgot Password";
                        //                        body = @"Click <a href='http://product.omniteaching.com/forgot?uid={1}&token={0}'>
                        //                            Reset Password
                        //                        </a>
                        //                        Or go to http://product.omniteaching.com/forgot?uid={1}&token={0}
                        //                        to change your password at FMS-E APP.";
                    }
                    if (acceptedLanguages.Any(x => x.Value == "zh-TW"))
                    {
                        lan = "zh-TW";
                        subject = "密碼報失";
                        //                        body = @"
                        //                        請點繫<a href='http://product.omniteaching.com/forgot?uid={1}&token={0}'>
                        //                            變更密碼
                        //                        </a>
                        //                        或進入以下地址 http://product.omniteaching.com/forgot?uid={1}&token={0}
                        //                        來更換您在 FMS-E APP 上的密碼";
                    }
                    //EmailHelper helper = new EmailHelper(user.Email, subject, string.Format(body, HttpUtility.UrlEncode(token), user.Id));
                    //helper.Send();


                    //var lan = _usr.Profile != null && !string.IsNullOrEmpty(_usr.Profile.Language) ? _usr.Profile.Language : "zh-CN";
                    //var subject = "对不起， 您的账户审核不通过";
                    var temp = "/Template/Mail/forgot_{0}.html";
                    //if (lan == "en-US")
                    //{
                    //    lan = "en-US";
                    //    subject = "Forgot Password";
                    //}
                    //if (lan == "zh-TW")
                    //{
                    //    lan = "zh-TW";
                    //    subject = "密碼報失";
                    //}
                    var file = string.Format(temp, lan);
                    var body = MyConsole.ReadFile(file);
                    EmailHelper helper = new EmailHelper(user.Email, subject, body.Replace("{token}", HttpUtility.UrlEncode(token ?? "")).Replace("{userid}", user.Id ?? "").Replace("{username}", user.FullName ?? "").Replace("{domain}", domain));
                    helper.Send();

                }
                return Ok();
            }
            catch (Exception ex)
            {
                MyConsole.Log(ex, "Password Forgot");
                throw ex;
            }

        }


        [AllowAnonymous]
        [Route("identity/forgot")]
        public async Task<IHttpActionResult> PutForgot(ForgotModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                model.Token = HttpUtility.UrlDecode(model.Token);
                var result = await _repo.ResetForgottenPassword(model);
                if (!result.Succeeded) return BadRequest(string.Join(",", result.Errors));
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return Ok();
        }

        [AllowAnonymous]
        [Route("identity/check/{category}")]
        public async Task<IHttpActionResult> PostCheckExist(UserModel model, string category = "email")
        {
            if (category == "email")
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    return BadRequest();
                }
                String strExp = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                Regex r = new Regex(strExp);
                Match m = r.Match(model.Email);
                if (!m.Success) return BadRequest();

                var result = await _repo.FindUserByEmail(model.Email);
                if (result == null)
                {
                    return Ok();
                }
            }
            else if (category == "user")
            {
                if (string.IsNullOrEmpty(model.UserName))
                {
                    return BadRequest();
                }
                var result = await _repo.FindUserByName(model.UserName);
                if (result == null)
                {
                    return Ok();
                }
            }
            else if (category == "phone")
            {
                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    return BadRequest();
                }
                //String strExp = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                //Regex r = new Regex(strExp);
                //Match m = r.Match(model.Email);
                //if (!m.Success) return BadRequest();

                var result = await _repo.FindUserByPhone(model.PhoneNumber);
                if (result == null)
                {
                    return Ok();
                }
            }

            return BadRequest();
        }

        [AllowAnonymous]
        [Route("identity/confirmemail")]
        public async Task<IHttpActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }

            var result = await _repo.ConfirmEmail(userId, code);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest();
        }

        [Route("identity/changepassword")]
        public async Task<IHttpActionResult> ResetPassword(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await this.User.Identity.GetUser();
            model.ID = user.Id;
            model.UserName = user.UserName;
            IdentityResult result = await _repo.ChangePassword(model, "");
            IHttpActionResult error = GetErrorResult(result);
            if (error != null)
            {
                return error;
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }
            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {

            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    return BadRequest();
                }
                return BadRequest(ModelState);
            }
            return null;
        }

    }
}
