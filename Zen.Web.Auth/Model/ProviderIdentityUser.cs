using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.Auth.Model
{
    public class ProviderIdentityUser : Data<ProviderIdentityUser>, IDataId
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public IdentityUser IdentityUser { get; set; }
        public string ProviderName { get; set; }
        public string ProviderKey { get; set; }
        public string InternalId { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public string Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Label { get; set; }
        public string LoginProvider { get; set; }
    }
}