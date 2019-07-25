using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using Zen.App.Orchestrator.Model;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Cache;

namespace Zen.App.Provider
{
    public abstract class AppOrchestratorPrimitive<TA, TG, TP, TPerm> : IAppOrchestrator
        where TA : Data<TA>, IZenApplication
        where TG : Data<TG>, IZenGroup
        where TP : Data<TP>, IZenPerson
        where TPerm : Data<TPerm>, IZenPermission
    {
        private IZenApplication _application;
        private object _settings;

        #region Implementation of IZenProvider

        public void Initialize()
        {
            DetectEnvironment();
            _settings = new Settings().GetSettings();
        }

        #endregion

        #region Implementation of IAppOrchestrator

        public virtual object Settings => _settings;
        public virtual IZenPerson GetPersonByLocator(string locator) { return Data<TP>.GetByLocator(locator); }

        public virtual IZenGroup GetGroupByCode(string code) { return Data<TG>.GetByLocator(code); }

        public virtual IZenApplication GetApplicationByLocator(string locator) { return Data<TA>.GetByLocator(locator); }

        public IZenApplication GetNewApplication() { return Data<TA>.New(); }

        public IZenApplication UpsertApplication(IZenApplication application)
        {
            var temp = (Data<TA>) application;
            temp.Save();

            return (IZenApplication) temp;
        }

        public List<IZenGroup> GetFullHierarchicalChain(IZenGroup referenceGroup)
        {
            var baseType = referenceGroup.GetType().FullName + ".Hierarchy:";
            var key = baseType + referenceGroup.Id;

            var cached = Zen.Base.Current.Cache[key];

            if (cached != null)
            {
                return cached.FromJson<List<TG>>().Select(i=> (IZenGroup)i).ToList();
            }

            var entry = InternalGetFullHierarchicalChain(referenceGroup);

            Zen.Base.Current.Cache[key] = entry.ToJson();

            return entry;
        }

        internal List<IZenGroup> InternalGetFullHierarchicalChain(IZenGroup referenceGroup) { return InternalGetFullHierarchicalChain(referenceGroup, true); }

        internal List<IZenGroup> InternalGetFullHierarchicalChain(IZenGroup referenceGroup, bool ignoreParentWhenAppOwned)
        {
            var chain = new List<IZenGroup>();

            if (referenceGroup.ParentId != null)
                if ( string.IsNullOrEmpty(referenceGroup.ApplicationId) || !ignoreParentWhenAppOwned)
                {
                    var parent = Data<TG>.Get(referenceGroup.ParentId);
                    chain = GetFullHierarchicalChain(parent);
                }

            chain.Add(referenceGroup);

            return chain;
        }

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

        public IZenPermission GetPermissionByFullCode(string fullCode) { return Data<TPerm>.Where(i => i.FullCode == fullCode).FirstOrDefault(); }

        public List<IZenPerson> GetAllPeople() { return Data<TP>.All().Select(i => (IZenPerson) i).ToList(); }
        public void SavePerson(List<IZenPerson> people) { Data<TP>.Save(people.Select(i => (TP) i)); }

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

        private static IZenApplication GetCurrentApplication()
        {
            var initialSettings = Configuration.Options.GetSection("Application").Get<Application.Settings>();

            var appLocator = initialSettings?.Locator ?? Host.ApplicationAssemblyName + ".dll";

            var application = Current.Orchestrator.GetApplicationByLocator(appLocator);

            if (application != null) return application;

            // No app detected. 

            application = Current.Orchestrator.GetNewApplication();

            var settingsNonHostGroups = initialSettings?.Groups?.Where(i => !i.IsHost).ToList() ?? new List<Application.Settings.Group>();

            if (settingsNonHostGroups.Any())
            {
                // Host group is mandatory, so let's probe for it.

                var settingsHostGroup = initialSettings?.Groups?.FirstOrDefault(i => i.IsHost);
                if (settingsHostGroup == null) throw new ArgumentException("No Host group defined");

                var hostGroup = Current.Orchestrator.GetGroupByCode(settingsHostGroup.Code);
                if (hostGroup == null) throw new ArgumentException($"Invalid Host group code: {settingsHostGroup.Code}");
            }

            // Host group defined, so now we can start.

            application.Name = initialSettings?.Name ?? Host.ApplicationAssemblyName;
            application.Code = initialSettings?.Code ?? Host.ApplicationAssemblyName;
            application.Locator = initialSettings?.Locator ?? appLocator;

            application = Current.Orchestrator.UpsertApplication(application);

            return application;
        }

        #endregion
    }
}