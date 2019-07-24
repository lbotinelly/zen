using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using static Zen.App.Orchestrator.Model.Application;

namespace Zen.App.Provider
{
    public abstract class AppOrchestratorPrimitive<TA, TG, TP> : IAppOrchestrator
        where TA : Data<TA>, IZenApplication
        where TG : Data<TG>, IZenGroup
        where TP : Data<TP>, IZenPerson
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
            var temp = (Data<TA>)application;
            temp.Save();

            return (IZenApplication)temp;
        }

        public List<IZenGroup> GetFullHierarchicalChain(IZenGroup referenceGroup)
        {
            List<IZenGroup> chain;

            if (referenceGroup.ParentId != null)
            {
                var parent = Data<TG>.Get(referenceGroup.ParentId);
                chain = GetFullHierarchicalChain(parent);
            }
            else { chain = new List<IZenGroup>(); }

            chain.Add(referenceGroup);

            return chain;
        }

        public virtual List<Permission> GetPermissionsByPerson(IZenPerson person)
        {
            var ret = new List<Permission>();

            IEnumerable<IZenGroup> groups = person.Groups().WithParents().ToList();

            foreach (var zenGroup in groups) ret.AddRange(zenGroup.Permissions);

            return ret.DistinctBy(i => i.Id).ToList();
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

            var initialSettings = Configuration.Options.GetSection("Application").Get<Zen.App.Orchestrator.Model.Application.Settings>();

            var appLocator = initialSettings?.Locator ?? Host.ApplicationAssemblyName + ".dll";

            var application = Current.Orchestrator.GetApplicationByLocator(appLocator);

            if (application != null) return (IZenApplication)application;

            // No app detected. 

            application = Current.Orchestrator.GetNewApplication();

            var settingsNonHostGroups = initialSettings?.Groups?.Where(i => !i.IsHost).ToList() ?? new List<Zen.App.Orchestrator.Model.Application.Settings.Group>();

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

            return (IZenApplication)application;
        }

        #endregion
    }
}