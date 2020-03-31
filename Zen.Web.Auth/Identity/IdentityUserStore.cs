using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Auth.Model;

namespace Zen.Web.Auth.Identity
{
    public class IdentityUserStore : IUserEmailStore<IdentityUser>, IUserLoginStore<IdentityUser>
    {
        public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var probe = LoginInfo.Where(i => i.IdentityUser.NormalizedEmail == normalizedEmail).FirstOrDefault();
            return Task.FromResult(probe?.IdentityUser);
        }

        public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);

        public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var key = $"{login.LoginProvider}+{login.ProviderKey}";

            var externalLogin = (ExternalLoginInfo)login;

            var claimsIdentity = (ClaimsIdentity)externalLogin.Principal.Identity;

            var claims = claimsIdentity.Claims.ToList();

            var claimDict = new Dictionary<string,string>();

            foreach (var claim in claims.Where(claim => !claimDict.ContainsKey(claim.Type)))
                claimDict[claim.Type] = claim.Value;


            var entry = new LoginInfo
            {
                Id = key,
                IdentityUser = user,
                Name = claimsIdentity.Name,
                Claims = claimDict,
                IsAuthenticated = claimsIdentity.IsAuthenticated,
                Label = claimsIdentity.Label,

                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey
            };

            Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, $"Welcome, {entry.Name} ({entry.Id})", Message.EContentType.Info);

            entry.Save();

            return Task.CompletedTask;
        }

        public Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var key = $"{loginProvider}+{providerKey}";
            var probe = LoginInfo.Get(key);

            return Task.FromResult(probe?.IdentityUser);
        }

        public void Dispose() { }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            var key = user.NormalizedUserName.StringToGuid().ToString();

            var probe = TransientState.Get(key) ?? new TransientState { Id = key };
            probe.IdentityUser = user;
            probe.Save();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            var probe = LoginInfo.Where(i => i.IdentityUser.NormalizedEmail == user.NormalizedEmail).FirstOrDefault();

            if (probe == null) return Task.FromResult(IdentityResult.Failed());

            probe.IdentityUser = user;
            probe.Save();

            Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, $"Welcome back, {probe.Name} ({probe.Id})", Message.EContentType.Info);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(LoginInfo.Get(userId)?.IdentityUser);
        }

        public Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.FromResult(TransientState.Where(i => i.UserName == normalizedUserName).FirstOrDefault()?.IdentityUser);
        }
    }
}