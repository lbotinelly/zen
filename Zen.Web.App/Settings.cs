using System;
using Zen.Base;
using Zen.Base.Extension;

namespace Zen.Web.App
{
    static class Settings
    {
        internal static string InternalGetFrameworkSettings()
        {
            //if (_settings != null) return;
            if (Zen.App.Current.ApplicationProvider.Application == null) return null;
            var app = Zen.App.Current.ApplicationProvider.Application;

            try
            {
                var settings = new
                {
                    Assembly = Base.Host.ApplicationAssemblyName,
                    Application = new
                    {
                        app?.Code,
                        app?.Name,
                        app?.Locator,
                        app?.Active,
                        Version = Base.Host.ApplicationAssembly.GetName().Version.ToString(),
                        Groups = app?.GetGroups()
                    },
                    Client = new
                    {
                        Base.Current.State
                    },
                    Session = new
                    {
                        CanSignIn = true,
                        IsExternalSession = true,
                        IsSecureConnection = true,
                        IsImpersonated = false
                    },
                    ApiUri = "/",
                    ResourceUri = "/",
                    vTag = "?v=" + Base.Host.ApplicationAssembly.GetName().Version,
                    datasources = new { },
                    counters = new { }
                }.ToJson();

                return settings;
            }

            catch (Exception e)
            {
                Log.Add("SetFrameworkSettings FAIL. Dump:");
                Log.Add("   ApplicationAssemblyName : " + Base.Host.ApplicationAssemblyName);
                Log.Add("               Application : " + app);
                Log.Add("               Environment : " + Base.Current.Environment);
                // Zen.Base.Log.Add("                   Session : " + Zen.Web.Current.);
                Log.Add(e);

                throw;
            }
        }
    }
}