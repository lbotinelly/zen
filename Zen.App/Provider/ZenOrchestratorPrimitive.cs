using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Zen.App.Core.Application;
using Zen.App.Core.Group;
using Zen.App.Core.Person;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Service;
using Factory = Zen.App.Core.Person.Factory;

namespace Zen.App.Provider
{
    public abstract class ZenOrchestratorPrimitive<TA, TG, TP, TPerm> : IZenOrchestrator
        where TA : Data<TA>, IApplication
        where TG : Data<TG>, IGroup
        where TP : Data<TP>, IPerson
        where TPerm : Data<TPerm>, IPermission
    {
        private IApplication _application;

        #region Implementation of IZenProvider

        public virtual void Initialize()
        {
            DetectEnvironment();
            Settings = new Settings().GetSettings();
        }

        #endregion

        #region Implementation of IAppOrchestrator

        public virtual Dictionary<string, object> Settings { get; private set; }
        public virtual IPerson GetPersonByLocator(string locator) { return Data<TP>.GetByLocator(locator); }
        public virtual IEnumerable<IPerson> GetPeopleByLocators(IEnumerable<string> locators) { return Data<TP>.GetByLocator(locators); }

        public virtual IGroup GetGroupByCode(string code, string name = null, IApplication application = null, IGroup parent = null, bool createIfNotFound = false)
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

        public IApplication GetApplicationByCode(string code)
        {
            var probe = Data<TA>.GetByCode(code);
            return probe;
        }

        public virtual IApplication GetApplicationByLocator(string locator)
        {
            var probe = Data<TA>.GetByLocator(locator);

            return probe;
        }

        public IApplication GetNewApplication() { return Data<TA>.New(); }

        public IApplication UpsertApplication(IApplication application)
        {
            var temp = (Data<TA>) application;
            temp.Save();

            return (IApplication) temp;
        }

        public List<IGroup> GetFullHierarchicalChain(IGroup referenceGroup, bool ignoreParentWhenAppOwned = true)
        {
            var baseType = referenceGroup.GetType().FullName + ".Hierarchy:";
            var key = $"{baseType}{referenceGroup.Id}/{(ignoreParentWhenAppOwned ? "-app-isolated" : "")}";

            var cached = Base.Current.Cache[key];

            if (cached != null) return cached.FromJson<List<TG>>().Select(i => (IGroup) i).ToList();

            var chain = new List<IGroup>();

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

        public List<IGroup> GroupsByApplication(string key) { return Data<TG>.Where(i => i.ApplicationId == key).Select(i => (IGroup) i).ToList(); }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly char[] PermissionExpressionDelimiters = {',', ';', '\n'};

        private const string IsAuthenticatedPermission = "$ISAUTHENTICATED";

        public bool HasAnyPermissions(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return true;

            if (expression == IsAuthenticatedPermission)
                if (Person != null)
                    return true;

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

        public IPermission GetPermissionByCode(string code, string name = null, IApplication application = null, bool createIfNotFound = false)
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

        public List<IPerson> GetPeople(IEnumerable<string> keySet = null)
        {
            var set = keySet != null ? Data<TP>.GetByLocator(keySet) : Data<TP>.All();

            return set.Select(i => (IPerson) i).ToList();
        }

        public void SavePerson(List<IPerson> people) { Data<TP>.Save(people.Select(i => (TP) i)); }
        public virtual string GetApiUri() { throw new NotImplementedException(); }
        public virtual string GetResourceUri() { throw new NotImplementedException(); }

        public virtual List<IPerson> PeopleByGroup(string key) { return Person.ByGroup(key); }

        private readonly Type _defaultProfileType = IoC.GetClassesByInterface<IPersonProfile>(false).FirstOrDefault();

        public virtual List<IPersonProfile> GetProfiles(string keys)
        {
            if (keys == null) return null;

            var buffer = new List<IPersonProfile>();

            var keySet = keys?.Split(',').ToList();

            var people = GetPeople(keySet);

            foreach (var zenPerson in people) buffer.Add(GetProfile(zenPerson));

            var orderedOutput = new Dictionary<string, IPersonProfile>();
            foreach (var key in keySet) orderedOutput[key] = buffer.FirstOrDefault(i => i.Locator == key);

            return orderedOutput.Values.ToList();
        }

        public virtual IPersonProfile GetProfile(IPerson person)
        {
            var profile = _defaultProfileType.CreateInstance<IPersonProfile>();
            profile.FromPerson(person);

            return profile;
        }

        public IApplication GetApplicationById(string identifier)
        {
            var probe = Data<TA>.Get(identifier);

            return probe;
        }

        public IPerson GetPersonByEmail(string email)
        {
            email = email.ToLower().Trim();
            return Data<TP>.Where(i => i.Email.ToLower() == email).FirstOrDefault();
        }

        public virtual List<IPermission> GetPermissionsByPerson(IPerson person)
        {
            var keys = new List<string>();

            IEnumerable<IGroup> groups = person.Groups().WithParents().ToList();

            foreach (var zenGroup in groups) keys.AddRange(zenGroup.Permissions);

            keys = keys.Distinct().ToList();

            var permissions = Data<TPerm>.Get(keys).Select(i => (IPermission) i).ToList();

            return permissions;
        }

        public virtual IPerson SigninPersonByIdentity(IIdentity userIdentity) { throw new NotImplementedException(); }
        public virtual void SignInPerson(IPerson person) { throw new NotImplementedException(); }

        public IPerson Person => Factory.Current;
        public virtual IApplication Application => _application;

        public void DetectEnvironment()
        {
            // Let's determine the current app.

            if (_application != null) return;

            _application = Core.Application.Factory.Current;

            Events.AddLog("Application", _application.ToString());
        }

        #endregion
    }
}