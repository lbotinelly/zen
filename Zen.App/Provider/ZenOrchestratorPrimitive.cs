using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Zen.App.Provider.Application;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.App.Provider
{
    public abstract class ZenOrchestratorPrimitive<TA, TG, TP, TPerm> : IZenOrchestrator
        where TA : Data<TA>, IZenApplication
        where TG : Data<TG>, IZenGroup
        where TP : Data<TP>, IZenPerson
        where TPerm : Data<TPerm>, IZenPermission
    {
        private IZenApplication _application;

        #region Implementation of IZenProvider

        public virtual void Initialize()
        {
            DetectEnvironment();
            Settings = new Settings().GetSettings();
        }

        #endregion

        #region Implementation of IAppOrchestrator

        public virtual Dictionary<string, object> Settings { get; private set; }
        public virtual IZenPerson GetPersonByLocator(string locator) { return Data<TP>.GetByLocator(locator); }
        public virtual IZenGroup GetGroupByCode(string code, string name = null, IZenApplication application = null, IZenGroup parent = null, bool createIfNotFound = false)
        {
            var probe = Data<TG>.GetByCode(code);

            if (probe != null) return probe;

            if (!createIfNotFound) return null;

            var referenceApplication = application ?? Application;

            probe = typeof(TG).CreateInstance<TG>();

            probe.ApplicationId = referenceApplication.Id;
            probe.ParentId = parent?.Id;
            probe.Code = code;
            probe.Active = true;
            probe.Name = name ?? code;
            probe.Save();

            return probe;
        }

        public virtual IZenApplication GetApplicationByLocator(string locator)
        {
            var probe = Data<TA>.GetByLocator(locator);

            return probe;
        }

        public IZenApplication GetNewApplication() { return Data<TA>.New(); }

        public IZenApplication UpsertApplication(IZenApplication application)
        {
            var temp = (Data<TA>) application;
            temp.Save();

            return (IZenApplication) temp;
        }

        public List<IZenGroup> GetFullHierarchicalChain(IZenGroup referenceGroup, bool ignoreParentWhenAppOwned = true)
        {
            var baseType = referenceGroup.GetType().FullName + ".Hierarchy:";
            var key = $"{baseType}{referenceGroup.Id}/{(ignoreParentWhenAppOwned ? "-app-isolated" : "")}";

            var cached = Base.Current.Cache[key];

            if (cached != null) return cached.FromJson<List<TG>>().Select(i => (IZenGroup) i).ToList();

            var chain = new List<IZenGroup>();

            if (referenceGroup.ParentId != null)
                if (string.IsNullOrEmpty(referenceGroup.ApplicationId) || !ignoreParentWhenAppOwned)
                {
                    var parent = Data<TG>.Get(referenceGroup.ParentId);
                    chain = GetFullHierarchicalChain(parent, ignoreParentWhenAppOwned);
                }

            chain.Add(referenceGroup);

            Base.Current.Cache[key] = chain.ToJson();

            return chain;
        }

        public List<IZenGroup> GroupsByApplication(string key) { return Data<TG>.Where(i => i.ApplicationId == key).Select(i => (IZenGroup) i).ToList(); }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly char[] PermissionExpressionDelimiters = {',', ';', '\n'};

        public bool HasAnyPermissions(string expression)
        {
            var permissionList = expression.Split(PermissionExpressionDelimiters, StringSplitOptions.RemoveEmptyEntries);
            return HasAnyPermissions(permissionList);
        }

        public bool HasAnyPermissions(IEnumerable<string> terms)
        {
            var appCodeMatric = $"[{Application.Code}].[{{0}}]";

            var matchingPermissions = terms
                .Select(i => i.StartsWith('[') ? i : string.Format(appCodeMatric, i))
                .ToList();

            return Person?.Permissions.Intersect(matchingPermissions).Any() == true;
        }

        public IZenPermission GetPermissionByCode(string code, string name = null, IZenApplication application = null, bool createIfNotFound = false)
        {
            var referenceApplication = application ?? Application;

            var fullCode = code.StartsWith('[') ? code : $"[{referenceApplication.Code}].[{code}]";

            var probe = Data<TPerm>.Where(i => i.FullCode == fullCode).FirstOrDefault();

            if (probe != null) return probe;

            if (!createIfNotFound) return null;

            probe = typeof(TPerm).CreateInstance<TPerm>();
            probe.ApplicationId = referenceApplication.Id;
            probe.Code = code;
            probe.FullCode = fullCode;
            probe.Name = name ?? code;
            probe.Save();

            return probe;
        }

        public List<IZenPerson> GetAllPeople() { return Data<TP>.All().Select(i => (IZenPerson) i).ToList(); }
        public void SavePerson(List<IZenPerson> people) { Data<TP>.Save(people.Select(i => (TP) i)); }
        public virtual string GetApiUri() { throw new NotImplementedException(); }
        public virtual string GetResourceUri() { throw new NotImplementedException(); }

        public virtual List<IZenPerson> PeopleByGroup(string key) { return Person.ByGroup(key); }

        public virtual List<IZenPermission> GetPermissionsByPerson(IZenPerson person)
        {
            var keys = new List<string>();

            IEnumerable<IZenGroup> groups = person.Groups().WithParents().ToList();

            foreach (var zenGroup in groups) keys.AddRange(zenGroup.Permissions);

            keys = keys.Distinct().ToList();

            var permissions = Data<TPerm>.Get(keys).Select(i => (IZenPermission) i).ToList();

            return permissions;
        }

        public virtual IZenPerson SigninPersonByIdentity(IIdentity userIdentity) { throw new NotImplementedException(); }
        public virtual void SignInPerson(IZenPerson person) { throw new NotImplementedException(); }

        public virtual IZenPerson Person { get; }
        public virtual IZenApplication Application => _application;

        public void DetectEnvironment()
        {
            // Let's determine the current app.

            if (_application != null) return;

            _application = GetCurrentApplication();
        }

        private static IZenApplication GetCurrentApplication() { return Factory.GetCurrentApplication(); }

        #endregion
    }
}