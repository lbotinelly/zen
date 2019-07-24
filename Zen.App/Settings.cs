﻿using System;
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