using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.App.Provider;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Zen.App.Orchestrator.Model
{
    public class Group : Data<Group>, IZenGroup
    {
        public string Name { get; set; }

        #region Implementation of IDataActive

        public bool IsActive { get; set; } = true;

        #endregion

        public string ApplicationId { get; set; }

        public void AddPermission(IZenPermission targetPermission)
        {
            if (Permissions.Contains(targetPermission.Id)) return;
            Permissions.Add(targetPermission.Id);
            Save();
            Base.Current.Log.KeyValuePair(ToString(), $"+ {targetPermission.FullCode}", Message.EContentType.Info);
        }

        public void RemovePermission(IZenPermission targetPermission)
        {
            if (!Permissions.Contains(targetPermission.Id)) return;
            Permissions.Remove(targetPermission.Id);
            Save();

            Base.Current.Log.KeyValuePair(ToString(), $"- {targetPermission.FullCode}", Message.EContentType.Info);
        }

        public void AddPerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false)
        {
            var probe = GroupSubscription.Where(i => (i.GroupId == Id) & (i.PersonId == person.Id)).FirstOrDefault();

            if (probe != null)
                if (probe.Active)
                    return;

            if (probe != null)
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

            Base.Current.Log.KeyValuePair(person.Locator, "ADD to " + ToString(), Message.EContentType.Info);
        }

        public void RemovePerson(IZenPerson person, bool automated = false, bool useNonAutomatedIfFound = false)
        {
            var probe = GroupSubscription.Where(i => (i.GroupId == Id) & (i.PersonId == person.Id)).FirstOrDefault();

            if (probe == null) return;

            if (automated)
                if (!probe.IsImport)
                    if (!useNonAutomatedIfFound)
                        return;

            probe.Remove();

            Base.Current.Log.KeyValuePair(person.Locator, "REMOVE from " + ToString(), Message.EContentType.Info);
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

        private List<Person> GetPeople()
        {
            var subscribedPersonIds = GroupSubscription
                .Where(i => i.GroupId == Id && i.Active)
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
        public bool FromSettings { get; set; }

        #endregion
    }
}