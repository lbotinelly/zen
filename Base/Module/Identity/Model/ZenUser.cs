using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Zen.Base.Module.Identity.Model
{
    public class ZenUser : Data<ZenUser>
    {
        public enum EState
        {
            None,
            Initialized
        }

        public ZenUser()
        {
            Roles = new List<string>();
            Claims = new List<IdentityUserClaim<string>>();
            Logins = new List<IdentityUserLogin<string>>();
            Tokens = new List<IdentityUserToken<string>>();
            RecoveryCodes = new List<TwoFactorRecoveryCode>();
        }

        [Key]
        public string Id { get; internal set; }
        public EState State { get; set; } = EState.None;
        public string AuthenticatorKey { get; set; }
        public List<string> Roles { get; set; }
        public List<IdentityUserClaim<string>> Claims { get; set; }
        public List<IdentityUserLogin<string>> Logins { get; set; }
        public List<IdentityUserToken<string>> Tokens { get; set; }
        public List<TwoFactorRecoveryCode> RecoveryCodes { get; set; }
        [Display]
        public string UserName { get; set; }
        public string Email { get; set; }
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
        public virtual bool HasAnyPermissions(string perm) { return true; }
    }
}