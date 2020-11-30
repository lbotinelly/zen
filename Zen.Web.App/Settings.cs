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
                    Server = new
                    {
                        Environment.MachineName,
                        Environment.Version
                    },
                    Client = new
                    {
                        Base.Current.State
                    },
                    //Setup.Auth.Environment.BasePath,
                    Environment = new
                    {
                        Base.Current.Environment.CurrentCode,
                        Base.Current.Environment.Current.Name
                    },
                    //Session = new
                    //{
                    //    Static = Session.IsExternalSession,
                    //    Session.CanSignIn,
                    //    Session.IsExternalSession,
                    //    Session.IsSecureConnection,
                    //    Session.IsImpersonated
                    //},
                    Session = new
                    {
                        Static = false,
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