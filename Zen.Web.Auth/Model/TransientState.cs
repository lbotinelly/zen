using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.Auth.Model
{
    public class TransientState : Data<TransientState>, IDataId
    {
        [Key] 
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public IdentityUser IdentityUser { get; set; }
        public bool Resolved { get; set; }
        public string NormalizedUserName { get; set; }
        public string UserName { get; set; }
        public string NormalizedEmail { get; set; }
        public string Email { get; set; }

        public static TransientState GetByIdentityUserId(string identityUserid) => Where(i => i.IdentityUser.Id == identityUserid).FirstOrDefault();
        public static TransientState GetByIdentityUser(IdentityUser user) => GetByIdentityUserId(user.Id);
    }
}