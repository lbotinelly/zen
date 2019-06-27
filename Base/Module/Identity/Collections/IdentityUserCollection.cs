using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module.Identity.Model;

namespace Zen.Base.Module.Identity.Collections
{
    public class IdentityUserCollection<TUser> : IIdentityUserCollection<TUser> where TUser : User
    {
        public async Task<TUser> FindByEmailAsync(string normalizedEmail) => await Task.Run(() => (TUser) User.Where(i => i.NormalizedEmail == normalizedEmail).FirstOrDefault());

        public async Task<TUser> FindByUserNameAsync(string username) => await Task.Run(()=> (TUser)User.Where(i => i.UserName == username).FirstOrDefault());

        public async Task<TUser> FindByNormalizedUserNameAsync(string normalizedUserName) => await Task.Run(() => (TUser) User.Where(i=> i.NormalizedUserName == normalizedUserName).FirstOrDefault());

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey) => (TUser) await Task.Run(() => User.Where(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)).FirstOrDefault());

        public async Task<IEnumerable<TUser>> FindUsersByClaimAsync(string claimType, string claimValue) => (IEnumerable<TUser>) await Task.Run(() => (TUser) User.Where(u => u.Claims.Any(c => c.ClaimType == claimType && c.ClaimValue == claimValue)));

        public async Task<IEnumerable<TUser>> FindUsersInRoleAsync(string roleName) => (IEnumerable<TUser>) await Task.Run(() => User.Where(u => u.Roles.Contains(roleName)));

        public async Task<IEnumerable<TUser>> GetAllAsync() => await Task.Run(() => (IEnumerable<TUser>) User.All());

        public async Task<TUser> CreateAsync(TUser obj) => await Task.Run(() => (TUser) obj.Save());

        public Task UpdateAsync(TUser obj) => Task.Run(() => (TUser) obj.Save());

        public Task DeleteAsync(TUser obj) => Task.Run(() => (TUser) obj.Remove());

        public Task<TUser> FindByIdAsync(string itemId) => Task.Run(() => (TUser) User.Get(itemId));
    }
}