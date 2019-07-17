using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.App.Provider;
using Zen.Base.Module;

namespace Zen.App.Orchestrator.Model
{
    public class User : Data<User>, IZenPerson
    {
        public enum EState
        {
            None,
            Initialized
        }

        public User()
        {
            Roles = new List<string>();
        }

        #region Implementation of IDataId
        [Key]
        public string Id { get; set; }
        #endregion
        #region Implementation of IDataLocator
        [Display]
        public string Locator { get; set; }
        #endregion
        public EState State { get; set; } = EState.None;
        public string AuthenticatorKey { get; set; }
        public List<string> Roles { get; set; }
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
        public List<IZenGroup> GetGroups() { throw new NotImplementedException(); }
        public virtual bool HasAnyPermissions(string perm) { return true; }
        public bool HasAnyPermissions(string[] terms) { throw new NotImplementedException(); }
    }
}