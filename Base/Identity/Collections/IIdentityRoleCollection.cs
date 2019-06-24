using System.Collections.Generic;
using System.Threading.Tasks;
using Zen.Base.Identity.Model;

namespace Zen.Base.Identity.Collections {
    public interface IIdentityRoleCollection<TRole> where TRole : ZenRole
    {
        Task<TRole> FindByNameAsync(string normalizedName);
        Task<TRole> FindByIdAsync(string roleId);
        Task<IEnumerable<TRole>> GetAllAsync();
        Task<TRole> CreateAsync(TRole obj);
        Task UpdateAsync(TRole obj);
        Task DeleteAsync(TRole obj);
    }
}