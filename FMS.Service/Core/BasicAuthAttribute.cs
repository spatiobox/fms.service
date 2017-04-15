using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;

namespace FMS.Service.Core
{
    public class BasicAuthAttribute : AuthorizeAttribute
    {

        //
        // Summary:
        //     Calls when an action is being authorized.
        //
        // Parameters:
        //   actionContext:
        //     The context.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     The context parameter is null.
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            //检验用户ticket信息，用户ticket信息来自调用发起方
            if (actionContext.Request.Headers.Authorization != null)
            {
                //解密用户ticket,并校验用户名密码是否匹配
                var encryptTicket = actionContext.Request.Headers.Authorization.Parameter;
                if (ValidateUserTicket(encryptTicket))
                    base.OnAuthorization(actionContext);
                else
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            else
            {
                //检查web.config配置是否要求权限校验
                //bool isRquired = (WebConfigurationManager.AppSettings["WebApiAuthenticatedFlag"].ToString() == "true");
                //if (isRquired)
                //{
                //    //如果请求Header不包含ticket，则判断是否是匿名调用
                var attr = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().OfType<AllowAnonymousAttribute>();
                bool isAnonymous = attr.Any(a => a is AllowAnonymousAttribute);

                //是匿名用户，则继续执行；非匿名用户，抛出“未授权访问”信息
                if (isAnonymous)
                    base.OnAuthorization(actionContext);
                else
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                //}
                //else
                //{
                //    base.OnAuthorization(actionContext);
                //}
            }
        }

        /// <summary>
        /// 校验用户ticket信息
        /// </summary>
        /// <param name="encryptTicket"></param>
        /// <returns></returns>
        private bool ValidateUserTicket(string encryptTicket)
        {
            var userTicket = FormsAuthentication.Decrypt(encryptTicket);
            var userTicketData = userTicket.UserData;

            string userName = userTicketData.Substring(0, userTicketData.IndexOf(":"));
            string password = userTicketData.Substring(userTicketData.IndexOf(":") + 1);

            //检查用户名、密码是否正确，验证是合法用户
            //var isQuilified = CheckUser(userName, password);
            return true;
        }

    }
}