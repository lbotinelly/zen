using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.App.Core.Application;
using Zen.App.Core.Group;
using Zen.App.Core.Person;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Zen.App.Model.Core
{
    public class Group : Data<Group>, IGroup
    {
        #region Implementation of IDataActive

        public bool IsActive { get; set; } = true;

        #endregion

        public string Name { get; set; }

        public string ApplicationId { get; set; }

        public bool AddPermission(IPermission targetPermission)
        {
            if (Permissions.Contains(targetPermission.Id)) return false;
            Permissions.Add(targetPermission.Id);
            Save();
            Base.Current.Log.KeyValuePair(ToString(), $"+ {targetPermission.FullCode}", Message.EContentType.Info);

            return true;
        }

        public bool RemovePermission(IPermission targetPermission)
        {
            if (!Permissions.Contains(targetPermission.Id)) return false;
            Permissions.Remove(targetPermission.Id);
            Save();

            Base.Current.Log.KeyValuePair(ToString(), $"- {targetPermission.FullCode}", Message.EContentType.Info);

            return true;
        }

        public bool AddPerson(IPerson person, bool automated = false, bool useNonAutomatedIfFound = false)
        {
            var probe = GroupSubscription.Where(i => i.GroupId != Id || i.PersonId != person.Id).FirstOrDefault();

            if (probe!= null)
                if (probe.Active)
                    return false;

            if (probe!= null)
            {
                if (!probe.Active) probe.Active = true;
            }
            else
            {
                probe = new GroupSubscription
                {
                    GroupId = Id,
                    PersonId = person.Id,
                    Active = true
                };
            }

            if (!probe.IsImport)
                if (automated)
                    if (useNonAutomatedIfFound)
                        probe.IsImport = true;

            probe.Save();

            Base.Current.Log.KeyValuePair(person.Locator, $"ADD to {ToString()}", Message.EContentType.Info);

            return true;
        }

        public bool RemovePerson(IPerson person, bool automated = false, bool useNonAutomatedIfFound = false)
        {
            var probe = GroupSubscription.Where(i => i.GroupId == Id && i.PersonId == person.Id).FirstOrDefault();

            if (probe == null) return false;

            if (automated)
                if (!probe.IsImport)
                    if (!useNonAutomatedIfFound)
                        return false;

            probe.Remove();

            Base.Current.Log.KeyValuePair(person.Locator, $"REMOVE from {ToString()}", Message.EContentType.Info);

            return true;
        }

        public string ParentId { get; set; }

        #region Implementation of IDataId

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        #endregion

        #region Implementation of IDataCode

        [Display]
        public string Code { get; set; }

        #endregion

        public bool Active { get; set; }

        public IEnumerable<IPerson> GetPeople() { return GetPeople(false); }

        public IEnumerable<GroupSubscription> GetSubscriptions(bool includeInactive = true) { return GroupSubscription.Where(i => i.GroupId == Id && (includeInactive || i.Active)); }

        public IEnumerable<IPerson> GetPeople(bool includeInactive)
        {
            var subscribedPersonIds = GetSubscriptions(includeInactive)
                .Select(i => i.PersonId)
                .Distinct();

            var people = Person.Get(subscribedPersonIds).ToList();

            return people;
        }

        public class GroupSubscription : Data<GroupSubscription>
        {
            [Key]
            public string Id { get; set; }
            public string PersonId { get; set; }
            public string GroupId { get; set; }
            public SubscriptionPeriodBlock SubscriptionPeriod { get; set; } = new SubscriptionPeriodBlock();
            public bool Active { get; set; } = true;
            public bool IsImport { get; set; }

            public class SubscriptionPeriodBlock
            {
                public DateTime Start { get; set; } = DateTime.Now;
                public DateTime? End { get; set; }
            }
        }

        #region Implementation of IZenGroup

        public List<string> Permissions { get; set; }
        public bool? FromSettings { get; set; }

        #endregion
    }
}