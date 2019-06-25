using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;

namespace Zen.Base.Identity.Model
{
    public class ZenUser : Data<ZenUser>
    {
        [Key]
        public string Id { get; internal set; }

        public ZenUser()
        {
            Roles = new List<string>();
            Claims = new List<IdentityUserClaim<string>>();
            Logins = new List<IdentityUserLogin<string>>();
            Tokens = new List<IdentityUserToken<string>>();
            RecoveryCodes = new List<TwoFactorRecoveryCode>();
        }

        public string AuthenticatorKey { get; set; }
        public List<string> Roles { get; set; }
        public List<IdentityUserClaim<string>> Claims { get; set; }
        public List<IdentityUserLogin<string>> Logins { get; set; }
        public List<IdentityUserToken<string>> Tokens { get; set; }
        public List<TwoFactorRecoveryCode> RecoveryCodes { get; set; }
        public string UserName { get;  set; }
        public string Email { get;  set; }
        public string NormalizedUserName { get; internal set; }
        public bool EmailConfirmed { get; internal set; }
        public string NormalizedEmail { get; internal set; }
        public bool LockoutEnabled { get; internal set; }
        public int AccessFailedCount { get; internal set; }
        public DateTimeOffset? LockoutEnd { get; internal set; }
        public string PasswordHash { get; internal set; }
        public string PhoneNumber { get; internal set; }
        public bool PhoneNumberConfirmed { get; internal set; }
        public string SecurityStamp { get; internal set; }
        public bool TwoFactorEnabled { get; internal set; }
    }
}