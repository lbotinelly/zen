using System;
using System.Collections.Generic;
using System.Linq;
using Zen.App.Model.Configuration;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;

namespace Zen.App.Core.Application
{
    public abstract class DataApplicationProvider<T> : IApplicationProvider where T : Data<T>, IApplication
    {
        private IApplication _current;
        public virtual Data<T> NewApplication => typeof(T).CreateInstance<T>();
        public IApplication Application
        {
            get
            {
                if (_current!= null) return _current;

                _current = Compile();
                return _current;
            }
        }

        public IApplication Compile(bool force = false)
        {
            var appLocator = Current.Configuration?.Locator ?? Host.ApplicationAssemblyName + ".dll";

            var application = ByLocator(appLocator) ?? (IApplication) NewApplication;

            var currentAppDescription = application.ToJson();

            application.Name = Current.Configuration?.Name ?? Host.ApplicationAssemblyName;
            application.Code = Current.Configuration?.Code ?? Host.ApplicationAssemblyName;
            application.Locator = Current.Configuration?.Locator ?? appLocator;

            var newAppDescription = application.ToJson();

            if (currentAppDescription == newAppDescription && !force) return application;

            application = Commit(application);

            // Now let's handle groups and permissions.

            Log.KeyValuePair($"{application} Startup-Sequence", "Configuration: START", Message.EContentType.StartupSequence);

            var settingsHostGroup = Current.Configuration?.Groups?.FirstOrDefault(i => i.IsHost);
            var hostGroup = Current.Orchestrator.GetGroupByCode(settingsHostGroup?.Code);

            var settingsNonHostGroups = Current.Configuration?.Groups?.Where(i => !i.IsHost).ToList() ?? new List<ApplicationConfiguration.GroupDescriptor>();

            Log.KeyValuePair($"{application} Startup-Sequence", $"Groups: {settingsNonHostGroups.Count}", Message.EContentType.StartupSequence);

            if (settingsNonHostGroups.Any())
            {
                // Host group is mandatory, so let's probe for it.
                if (settingsHostGroup == null) throw new ArgumentException("No Host group defined");

                if (hostGroup == null) throw new ArgumentException($"Invalid Host group code: {settingsHostGroup.Code}");

                // Host group defined, so now we can start.
            }

            if (settingsHostGroup!= null)
            {
                if (settingsHostGroup.Permissions!= null)
                    foreach (var permissionCode in settingsHostGroup.Permissions)
                    {
                        var targetPermission = Current.Orchestrator.GetPermissionByCode(permissionCode, permissionCode, application, true);
                        hostGroup.AddPermission(targetPermission);
                    }

                if (settingsHostGroup.Members!= null)
                    foreach (var personLocator in settingsHostGroup.Members)
                    {
                        var targetPermission = Current.Orchestrator.GetPersonByLocator(personLocator);
                        hostGroup.AddPerson(targetPermission, true, true);
                    }
            }

            if (settingsNonHostGroups.Any())
                foreach (var groupDescriptor in settingsNonHostGroups)
                {
                    var code = $"APP_{application.Code}_{groupDescriptor.Code}";

                    var targetGroup = Current.Orchestrator.GetGroupByCode(code, groupDescriptor.Name, application, hostGroup, true);

                    if (groupDescriptor.Permissions!= null)
                        foreach (var permissionCode in groupDescriptor.Permissions)
                        {
                            var targetPermission = Current.Orchestrator.GetPermissionByCode(permissionCode, permissionCode, application, true);
                            targetGroup.AddPermission(targetPermission);
                        }

                    if (groupDescriptor.Members!= null)
                        foreach (var personLocator in groupDescriptor.Members)
                        {
                            var targetPermission = Current.Orchestrator.GetPersonByLocator(personLocator);
                            targetGroup.AddPerson(targetPermission, true, true);
                        }
                }

            Log.KeyValuePair($"{application} Startup-Sequence", "Configuration: END", Message.EContentType.StartupSequence);

            return application;
        }

        public IApplication ByCode(string code) { return Data<T>.GetByCode(code); }

        public IApplication ByLocator(string locator) { return Data<T>.GetByLocator(locator); }

        public IApplication Commit(IApplication model) { return ((Data<T>) model).Save(); }
    }
}