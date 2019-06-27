using System.Collections.Generic;
using System.Threading.Tasks;
using Zen.Base.Module.Identity.Model;

namespace Zen.Base.Module.Identity.Collections
{
    public interface IIdentityRoleCollection<TRole> where TRole : Role
    {
        Task<TRole> FindByNameAsync(string normalizedName);
        Task<TRole> FindByIdAsync(string roleId);
        Task<IEnumerable<TRole>> GetAllAsync();
        Task<TRole> CreateAsync(TRole obj);
        Task UpdateAsync(TRole obj);
        Task DeleteAsync(TRole obj);
    }
}