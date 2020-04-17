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
using Zen.Web.Auth.Extensions;

namespace Zen.Web.Auth.Identity
{
    public class IdentityUserStore : IUserEmailStore<IdentityUser>, IUserLoginStore<IdentityUser>
    {
        public Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { normalizedEmail }.ToJson(), Message.EContentType.Info);
                var probe = Model.Identity.Where(i => i.IdentityUser.NormalizedEmail == normalizedEmail).FirstOrDefault();
                return probe?.IdentityUser;
            }, cancellationToken);

        public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { user }.ToJson(), Message.EContentType.Info);
                return IdentityResult.Success;
            }, cancellationToken);

        public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { user }.ToJson(), Message.EContentType.Info);

                var probe = Model.Identity.Where(i => i.IdentityUser.NormalizedEmail == user.NormalizedEmail).FirstOrDefault();

                if (probe == null) throw new KeyNotFoundException();

                probe.IdentityUser = user;
                probe.Save();

                return IdentityResult.Success;
            }, cancellationToken);

        public Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { userId }.ToJson(), Message.EContentType.Info);

                return Model.Identity.Get(userId)?.IdentityUser;
            }, cancellationToken);

        public Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { normalizedUserName }.ToJson(), Message.EContentType.Info);
                return Model.Identity.Where(i => i.IdentityUser.UserName == normalizedUserName).FirstOrDefault()?.IdentityUser;
            }, cancellationToken);

        public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { user }.ToJson(), Message.EContentType.Info);
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { loginProvider, providerKey }.ToJson(), Message.EContentType.Info);

                var key = Pipeline.StampValue(loginProvider, providerKey);
                var probe = Model.Identity.Get(key);

                return probe?.IdentityUser;
            }, cancellationToken);

        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { user, login }.ToJson(), Message.EContentType.Info);

                var key = login.StampValue();

                var claimsIdentity = (ClaimsIdentity)((ExternalLoginInfo)login).Principal.Identity;
                var claimDict = new Dictionary<string, string>();

                foreach (var claim in claimsIdentity.Claims.ToList().Where(claim => !claimDict.ContainsKey(claim.Type)))
                    claimDict[claim.Type] = claim.Value;

                var entry = new Model.Identity
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

                entry.Save();
            }, cancellationToken);

        public Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { loginProvider, providerKey, user }.ToJson(), Message.EContentType.Info);
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            if (Base.Host.IsDevelopment) Base.Current.Log.KeyValuePair(MethodBase.GetCurrentMethod().Name, new { user }.ToJson(), Message.EContentType.Info);
            throw new NotImplementedException();
        }

        #region Silly getters and setters

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);
        public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);
        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);

        public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken) => Task.Run(() => { user.Email = email; }, cancellationToken);
        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken) => Task.Run(() => { user.EmailConfirmed = confirmed; }, cancellationToken);
        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken) => Task.Run(() => { user.UserName = userName; }, cancellationToken);
        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken) => Task.Run(() => { user.NormalizedUserName = normalizedName; }, cancellationToken);
        public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken) => Task.Run(() => { user.NormalizedEmail = normalizedEmail; }, cancellationToken);
        public void Dispose() { }

        #endregion
    }
}