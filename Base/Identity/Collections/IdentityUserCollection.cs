using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Identity.Model;

namespace Zen.Base.Identity.Collections
{
    public class IdentityUserCollection<TUser> : IIdentityUserCollection<TUser> where TUser : ZenUser
    {
        public async Task<TUser> FindByEmailAsync(string normalizedEmail) { return await Task.Run(() => (TUser) ZenUser.Query(new {NormalizedEmail = normalizedEmail}.ToJson()).FirstOrDefault()); }

        public async Task<TUser> FindByUserNameAsync(string username) { return await Task.Run(() => (TUser) ZenUser.Query(new {UserName = username}.ToJson()).FirstOrDefault()); }

        public async Task<TUser> FindByNormalizedUserNameAsync(string normalizedUserName) { return await Task.Run(() => (TUser) ZenUser.Query(new {NormalizedUserName = normalizedUserName}.ToJson()).FirstOrDefault()); }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey) { return (TUser) await Task.Run(() => ZenUser.Where(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)).FirstOrDefault()); }

        public async Task<IEnumerable<TUser>> FindUsersByClaimAsync(string claimType, string claimValue) { return (IEnumerable<TUser>) await Task.Run(() => (TUser) ZenUser.Where(u => u.Claims.Any(c => c.ClaimType == claimType && c.ClaimValue == claimValue))); }

        public async Task<IEnumerable<TUser>> FindUsersInRoleAsync(string roleName) { return (IEnumerable<TUser>) await Task.Run(() => ZenUser.Where(u => u.Roles.Contains(roleName))); }

        public async Task<IEnumerable<TUser>> GetAllAsync() { return await Task.Run(() => (IEnumerable<TUser>) ZenUser.All()); }

        public async Task<TUser> CreateAsync(TUser obj) { return await Task.Run(() => (TUser) obj.Save()); }

        public Task UpdateAsync(TUser obj) { return Task.Run(() => (TUser) obj.Save()); }

        public Task DeleteAsync(TUser obj) { return Task.Run(() => (TUser) obj.Remove()); }

        public Task<TUser> FindByIdAsync(string itemId) { return Task.Run(() => (TUser) ZenUser.Get(itemId)); }
    }
}