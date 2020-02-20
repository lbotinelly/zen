using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.App.Core.Group;
using Zen.App.Core.Person;
using Zen.Base.Module;

namespace Zen.App.Model.Core
{
    public class Person : Data<Person>, IPerson
    {
        public enum EState
        {
            None,
            Initialized
        }

        public Person() { Roles = new List<string>(); }

        public EState State { get; set; } = EState.None;
        public string AuthenticatorKey { get; set; }
        public List<string> Roles { get; set; }
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

        #region Implementation of IDataId

        [Key]
        public string Id { get; set; }

        #endregion

        #region Implementation of IDataLocator

        [Display]
        public string Locator { get; set; }

        #endregion

        public string Email { get; set; }
        public string Name { get; set; }
        public virtual bool HasAnyPermissions(string perm) { return true; }
        public bool HasAnyPermissions(IEnumerable<string> terms) { throw new NotImplementedException(); }
        public List<IPerson> ByGroup(string key) { throw new NotImplementedException(); }
        public List<string> Permissions { get; set; }

        List<IGroup> IPerson.Groups() { throw new NotImplementedException(); }

        #region Implementation of IDataActive

        public bool Active { get; set; } = true;

        #endregion
    }
}