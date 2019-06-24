using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Zen.Module.Web.Auth.Zen {
    public class ZenUserStore : IUserStore<ZenUser> {
        #region Implementation of IDisposable

        public void Dispose() { throw new NotImplementedException(); }

        #endregion

        #region Implementation of IUserStore<ZenUser>

        public Task<string> GetUserIdAsync(ZenUser user, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<string> GetUserNameAsync(ZenUser user, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task SetUserNameAsync(ZenUser user, string userName, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<string> GetNormalizedUserNameAsync(ZenUser user, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task SetNormalizedUserNameAsync(ZenUser user, string normalizedName, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<IdentityResult> CreateAsync(ZenUser user, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<IdentityResult> UpdateAsync(ZenUser user, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<IdentityResult> DeleteAsync(ZenUser user, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<ZenUser> FindByIdAsync(string userId, CancellationToken cancellationToken) { throw new NotImplementedException(); }
        public Task<ZenUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) { throw new NotImplementedException(); }

        #endregion
    }
}