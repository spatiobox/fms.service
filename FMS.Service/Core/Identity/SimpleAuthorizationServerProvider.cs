using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using FMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace FMS.Service.Core.Identity
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private OmsContext repo = new OmsContext();

        private AuthContext ctx;
        private UserManager<ApplicationUser> userMgr;


        public SimpleAuthorizationServerProvider()
        {
            ctx = new AuthContext();
            userMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ctx));
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {

            var scopes = (context.Parameters["scope"] ?? "").Split(' ');
            string client_id;
            string secret;


            if (context.TryGetBasicCredentials(out client_id, out secret) || context.TryGetFormCredentials(out client_id, out secret))
            {
                var username = "";
                var client = repo.Clients.FirstOrDefault(x => x.Secret == secret && x.ID == client_id);
                if (client == null || scopes.Count() <= 0)
                {
                    context.SetError("invalid_implicit_grant", "the client does not supported.");
                    return;
                }
                context.OwinContext.Set<Client>("oauth:client", client);
                username = client.User.UserName;
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                foreach (var scope in scopes)
                {
                    identity.AddClaim(new Claim("oauth:scope", scope));
                }
                identity.AddClaim(new Claim("oauth:client", client_id));
                identity.AddClaim(new Claim(ClaimTypes.Name, username));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, client.User.Id));
                //identity.AddClaim(new Claim(ClaimTypes.Role, ))
                context.Validated(client_id);
            }
            else
            {
                context.Validated();
            }
        }



        //public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        //{
        //    string clientId;
        //    string clientSecret;
        //    if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
        //        context.TryGetFormCredentials(out clientId, out clientSecret))
        //    {

        //        var client = repo.Clients.FirstOrDefault(x => x.Secret == clientSecret && x.ID == clientId);
        //        if (client!= null)
        //        {
        //            context.Validated();
        //        }
        //    }
        //    return Task.FromResult(0);
        //}

        public override Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {

            string username = "";
            string userid = "";
            OmsContext repo = new OmsContext();

            var client = repo.Clients.FirstOrDefault(x => x.ID == context.ClientId);
            if (client == null)
            {
                context.SetError("invalid_grant", "The client valid error.");
                return Task.Run(() => { });
            }
            //username = client.User.UserName;
            //var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            //foreach (var scope in scopes)
            //{
            //    identity.AddClaim(new Claim("urn:oauth:scope", scope));
            //}
            //identity.AddClaim(new Claim("urn:oauth:client", context.ClientId));
            //identity.AddClaim(new Claim("UserName", client.User.UserName));
            userid = client.User.Id;

            var oAuthIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
            //oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, "iOS App"));
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userid));
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, username));
            var ticket = new AuthenticationTicket(oAuthIdentity, new AuthenticationProperties());
            context.Validated(ticket);

            return base.GrantClientCredentials(context);
        }

        //public override Task Gran

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            AuthRepository repo = new AuthRepository();

            if (string.IsNullOrEmpty(context.UserName))
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ApplicationUser user = await repo.FindUser(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }
            if (user.Status == 0)
            {
                context.SetError("waiting_grant", "账号正在审核中");
                return;
            }
            if (user.Status < 0)
            {
                context.SetError("failed_grant", "账号审核不通过");
                return;
            }
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Role, string.Join(",", user.Roles.Select(x => x.RoleId))));

            context.Validated(identity);
        }
    }
}