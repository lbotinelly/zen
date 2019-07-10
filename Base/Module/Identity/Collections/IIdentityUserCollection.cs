﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Zen.Base.Module.Identity.Model;

namespace Zen.Base.Module.Identity.Collections
{
    public interface IIdentityUserCollection<TUser> where TUser : User
    {
        Task<TUser> FindByEmailAsync(string normalizedEmail);
        Task<TUser> FindByUserNameAsync(string username);
        Task<TUser> FindByNormalizedUserNameAsync(string normalizedUserName);
        Task<TUser> FindByLoginAsync(string loginProvider, string providerKey);
        Task<IEnumerable<TUser>> FindUsersByClaimAsync(string claimType, string claimValue);
        Task<IEnumerable<TUser>> FindUsersInRoleAsync(string roleName);
        Task<IEnumerable<TUser>> GetAllAsync();
        Task<TUser> CreateAsync(TUser obj);
        Task UpdateAsync(TUser obj);
        Task DeleteAsync(TUser obj);
        Task<TUser> FindByIdAsync(string itemId);
    }
}