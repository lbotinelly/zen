using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Identity.Model;

namespace Zen.Base.Identity.Collections
{
    public class IdentityUserCollection<TUser> : IIdentityUserCollection<TUser> where TUser : ZenUser
    {
        public IdentityUserCollection() { }

        public async Task<TUser> FindByEmailAsync(string normalizedEmail)
        {
            return await Task.Run(() => (TUser)ZenUser.Query(new { NormalizedEmail = normalizedEmail }.ToJson()).FirstOrDefault());
        }

        public async Task<TUser> FindByUserNameAsync(string username)
        {
            return await Task.Run(() => (TUser)ZenUser.Query(new { UserName = username }.ToJson()).FirstOrDefault());
        }

        public async Task<TUser> FindByNormalizedUserNameAsync(string normalizedUserName)
        {
            return await Task.Run(() => (TUser)ZenUser.Query(new { NormalizedUserName = normalizedUserName }.ToJson()).FirstOrDefault());
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey)
        {
            return null;
            //return await _users.FirstOrDefaultAsync(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
        }

        public async Task<IEnumerable<TUser>> FindUsersByClaimAsync(string claimType, string claimValue)
        {
            return null;

            //return await _users.WhereAsync(u => u.Claims.Any(c => c.ClaimType == claimType && c.ClaimValue == claimValue));
        }

        public async Task<IEnumerable<TUser>> FindUsersInRoleAsync(string roleName)
        {
            return null;

            //var filter = Builders<TUser>.Filter.AnyEq(x => x.Roles, roleName);
            //var res = await _users.FindAsync(filter);
            //return res.ToEnumerable();
        }

        public async Task<IEnumerable<TUser>> GetAllAsync()
        {
            return await Task.Run(() => (IEnumerable<TUser>)ZenUser.All());
        }

        public async Task<TUser> CreateAsync(TUser obj)
        {
            return await Task.Run(() => (TUser)obj.Save());
        }

        public Task UpdateAsync(TUser obj)
        {
            return Task.Run(() => (TUser)obj.Save());
        }

        public Task DeleteAsync(TUser obj)
        {
            return Task.Run(() => (TUser)obj.Remove());
        }

        public Task<TUser> FindByIdAsync(string itemId)
        {
            return Task.Run(() => (TUser)ZenUser.Get(itemId));
        }

    }
}