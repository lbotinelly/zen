﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module.Identity.Model;

namespace Zen.Base.Module.Identity.Collections
{
    public class IdentityRoleCollection<TRole> : IIdentityRoleCollection<TRole> where TRole : Role
    {
        public async Task<TRole> FindByNameAsync(string normalizedName) { return await Task.Run(() => (TRole) Role.Query(new {NormalizedName = normalizedName}.ToJson()).FirstOrDefault()); }

        public async Task<TRole> FindByIdAsync(string roleId) { return await Task.Run(() => (TRole) Role.Get(roleId)); }

        public async Task<IEnumerable<TRole>> GetAllAsync() { return await Task.Run(() => (IEnumerable<TRole>) Role.All()); }

        public async Task<TRole> CreateAsync(TRole obj) { return await Task.Run(() => (TRole) obj.Save()); }

        public Task UpdateAsync(TRole obj) { return Task.Run(() => (TRole) obj.Save()); }

        public Task DeleteAsync(TRole obj) { return Task.Run(() => obj.Remove()); }
    }
}