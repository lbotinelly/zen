using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;

namespace Zen.App.Provider
{
    public abstract class AppOrchestratorPrimitive<TA, TG, TP, T> : IAppOrchestrator<T> where T : IZenPermission
        where TA : Data<TA>, IZenApplication<T>
        where TG : Data<TG>, IZenGroup<T>
        where TP : Data<TP>, IZenPerson<T>
    {
        private IZenApplication<T> _application;
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
        public virtual IZenPerson<T> GetPersonByLocator(string locator) { return Data<TP>.GetByLocator(locator); }
        public virtual IZenGroup<T> GetGroupByCode(string code) { return Data<TG>.GetByLocator(code); }
        public List<IZenGroup<T>> GetFullHierarchicalChain<T>(IZenGroup<T> referenceGroup) where T : IZenPermission { throw new NotImplementedException(); }
        public virtual IZenApplication<T> GetApplicationByLocator(string locator) { return Data<TA>.GetByLocator(locator); }
        public IZenApplication<T> GetNewApplication() { return Data<TA>.New(); }

        public IZenApplication<T> UpsertApplication(IZenApplication<T> application)
        {
            var temp = (Data<TA>)application;
            temp.Save();

            return (IZenApplication<T>)temp;
        }

        public List<IZenGroup<T>> GetFullHierarchicalChain(IZenGroup<T> referenceGroup)
        {
            List<IZenGroup<T>> chain;

            if (referenceGroup.ParentId != null)
            {
                var parent = Data<TG>.Get(referenceGroup.ParentId);
                chain = GetFullHierarchicalChain(parent);
            }
            else { chain = new List<IZenGroup<T>>(); }

            chain.Add(referenceGroup);

            return chain;
        }

        public virtual List<T> GetPermissionsByPerson(IZenPerson<T> person)
        {
            var ret = new List<T>();

            IEnumerable<IZenGroup<T>> groups = person.Groups().WithParents().ToList();

            foreach (var zenGroup in groups) ret.AddRange(zenGroup.Permissions);

            return ret.DistinctBy(i => i.Id).ToList();
        }

        public virtual IZenPerson<T> SigninPersonByIdentity(IIdentity userIdentity) { throw new NotImplementedException(); }
        public virtual void SignInPerson(IZenPerson<T> person) { throw new NotImplementedException(); }

        public virtual IZenPerson<T> Person { get; }
        public virtual IZenApplication<T> Application => _application;

        public void DetectEnvironment()
        {
            // Let's determine the current app.

            if (_application != null) return;

            _application = GetCurrentApplication();
        }

        private static IZenApplication<T> GetCurrentApplication()
        {

            var initialSettings = Configuration.Options.GetSection("Application").Get<Zen.App.Orchestrator.Model.Application.Settings>();

            var appLocator = initialSettings?.Locator ?? Host.ApplicationAssemblyName + ".dll";

            var application = Current.Orchestrator.GetApplicationByLocator(appLocator);

            if (application != null) return (IZenApplication<T>)application;

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

            return (IZenApplication<T>)application;
        }

        #endregion
    }
}