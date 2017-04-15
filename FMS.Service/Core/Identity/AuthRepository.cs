using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using FMS.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FMS.Service.Core.Identity
{
    public class AuthRepository : IDisposable
    {
        private AuthContext ctx;
        private UserStore<ApplicationUser> _uStore;
        private static UserManager<ApplicationUser> userMgr;

        public AuthRepository()
        {
            ctx = new AuthContext();
            _uStore = new UserStore<ApplicationUser>(ctx);
            userMgr = new UserManager<ApplicationUser>(_uStore);
            userMgr.UserValidator = new UserValidator<ApplicationUser, string>(userMgr)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true,
            };

            //userMgr.SupportsUserSecurityStamp

            // 配置用户锁定默认值
            userMgr.UserLockoutEnabledByDefault = true;
            userMgr.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            userMgr.MaxFailedAccessAttemptsBeforeLockout = 5;
            userMgr.UserTokenProvider = TokenProvider.Provider;
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            using (var tran = ctx.Database.BeginTransaction())
            {
                try
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        Company = userModel.Company,
                        FullName = userModel.FullName ?? "",
                        Email = userModel.Email,
                        PhoneNumber = userModel.PhoneNumber,
                        UserName = userModel.UserName,
                        Status = 0, //0:审核中， 1：审核通过， 2：审核不通过
                        Position = userModel.Position ?? "",
                        Department = userModel.Department ?? "",
                        Remark = userModel.Remark
                    };
                    var result = await userMgr.CreateAsync(user, userModel.Password);
                    if (result.Succeeded)
                    {
                        userMgr.AddToRole(user.Id, "customer");
                        userMgr.SetLockoutEnabled(user.Id, true);
                        userMgr.SetLockoutEndDate(user.Id, DateTimeOffset.MaxValue);
                    }
                    tran.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }


        public async Task<IdentityResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return IdentityResult.Failed("验证不通过，或者验证码已失效！");
            }

            var result = await userMgr.ConfirmEmailAsync(userId, code);
            return result;
        }

        public async Task<IdentityResult> ChangePassword(UserModel userModel, string OldPassword)
        {
            await userMgr.RemovePasswordAsync(userModel.ID);
            var result = await userMgr.AddPasswordAsync(userModel.ID, userModel.Password);
            return result;

        }

        [Flags]
        public enum AccountType
        {
            UserName = 0x1,
            Email = 0x2,
            PhoneNumber = 0x4
        }

        public async Task<ApplicationUser> FindUser(string userName, string password, AccountType type = AccountType.UserName)
        {

            ApplicationUser user = _uStore.Users.FirstOrDefault(x => x.PhoneNumber == userName || x.UserName == userName || x.Email == userName);
            if (user != null) user = await userMgr.FindAsync(user.UserName, password);

            return user;
        }



        public async Task<IdentityResult> UnlockoutUser(string userid)
        {
            IdentityResult result = await userMgr.SetLockoutEndDateAsync(userid, DateTimeOffset.Now);
            return result;
        }


        public async Task<ApplicationUser> FindUserByName(string userName)
        {
            ApplicationUser user = await userMgr.FindByNameAsync(userName);
            return user;
        }

        public async Task<ApplicationUser> FindUserByPhone(string phone)
        {
            ApplicationUser user = userMgr.Users.FirstOrDefault(x => x.PhoneNumber == phone);
            return user;
        }


        public async Task<ApplicationUser> FindUserById(string userid)
        {
            ApplicationUser user = await userMgr.FindByIdAsync(userid);
            return user;
        }

        public async Task<ApplicationUser> FindUserByEmail(string email)
        {
            ApplicationUser user = await userMgr.FindByEmailAsync(email);
            return user;
        }

        public async Task<string> GeneratePasswordResetToken(string userid)
        {
            return await userMgr.GeneratePasswordResetTokenAsync(userid);
        }

        public async Task<IdentityResult> ResetForgottenPassword(ForgotModel model)
        {
            var result = await userMgr.ResetPasswordAsync(model.UserID, model.Token, model.Password);
            return result;
        }

        public async Task<bool> IsLockedOutAsync(string userid)
        {
            var result = await userMgr.IsLockedOutAsync(userid);
            return result;
        }

        public void Dispose()
        {
            ctx.Dispose();
            userMgr.Dispose();
        }
    }


    public static class TokenProvider
    {
        //[UsedImplicitly]
        private static DataProtectorTokenProvider<ApplicationUser> _tokenProvider;

        public static DataProtectorTokenProvider<ApplicationUser> Provider
        {
            get
            {
                if (_tokenProvider != null) return _tokenProvider;
                var dataProtectionProvider = new DpapiDataProtectionProvider();
                _tokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create());
                return _tokenProvider;
            }
        }
    }
}