using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Zen.App.Orchestrator.Model;
using Zen.App.Provider;
using Zen.Base;

namespace Zen.App
{
    public class Settings
    {
        public object GetSettings()
        {
            //if (_settings != null) return;
            if (Current.Orchestrator.Application == null) return null;

            var settings = new
            {
                Assembly = Host.ApplicationAssemblyName,
                Application = new ApplicationSection(),
                Server = new
                {
                    Environment.MachineName,
                    Environment.Version
                },
                Environment = new
                {
                    Base.Current.Environment.Current.Code,
                    Base.Current.Environment.Current.Name
                },
                ResourceUri = "",
                ApiUri = "",
                vTag = "?v=" + Host.ApplicationAssemblyVersion,
                Session = new
                {
                    Static = false,
                    CanSignIn = true,
                    IsExternalSession = false,
                    IsSecureConnection = true,
                    IsImpersonated = false
                },
            };

            return settings;
        }

        public static IZenApplication GetCurrentApplication()
        {

            var initialSettings = Configuration.Options.GetSection("Application").Get<Application.Settings>();

            var appLocator = initialSettings.Locator ?? Host.ApplicationAssemblyName + ".dll";

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

            application.Name = initialSettings.Name;
            application.Code = initialSettings.Code;
            application.Locator = initialSettings.Locator;

            application = Current.Orchestrator.UpsertApplication(application);

            return application;

        }

        public class ApplicationSection
        {
            public bool Active;
            public string Code;
            public Application.ConfigurationBlock Configuration;
            public string Locator;
            public string Name;
            public string Version;

            public ApplicationSection()
            {
                var app = Current.Orchestrator.Application;

                Code = app.Code;
                Name = app.Name;
                Locator = app.Locator;
                Active = app.Active;
                Version = Host.ApplicationAssembly.GetName().Version.ToString();
            }
        }
    }
}