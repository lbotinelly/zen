using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Identity.Model;

namespace Zen.Base.Identity.Collections
{
    public class IdentityRoleCollection<TRole> : IIdentityRoleCollection<TRole> where TRole : ZenRole
    {
        public async Task<TRole> FindByNameAsync(string normalizedName) { return await Task.Run(() => (TRole) ZenRole.Query(new {NormalizedName = normalizedName}.ToJson()).FirstOrDefault()); }

        public async Task<TRole> FindByIdAsync(string roleId) { return await Task.Run(() => (TRole) ZenRole.Get(roleId)); }

        public async Task<IEnumerable<TRole>> GetAllAsync() { return await Task.Run(() => (IEnumerable<TRole>) ZenRole.All()); }

        public async Task<TRole> CreateAsync(TRole obj) { return await Task.Run(() => (TRole) obj.Save()); }

        public Task UpdateAsync(TRole obj) { return Task.Run(() => (TRole) obj.Save()); }

        public Task DeleteAsync(TRole obj) { return Task.Run(() => obj.Remove()); }
    }
}