using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zen.Base.Assembly;
using Zen.Base.Common;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Default;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Identity;
using Zen.Base.Module.Log;

namespace Zen.Base
{
    public static class Current
    {
        static Current()
        {
            Services = new ServiceCollection()
                .AddLogging(b =>
                {
                    b.AddConsole(i=> i.IncludeScopes = true);
                    b.AddDebug();
                })
                .ResolveSettingsPackage();

            ServiceProvider = Services.BuildServiceProvider();

            var providers = Services.Where(i => typeof(IZenProvider).IsAssignableFrom(i.ServiceType)).ToList();

            foreach (var zenService in providers)
            {
                ((IZenProvider)ServiceProvider.GetService(zenService.ServiceType)).Initialize();
            }

            Events.Start();

            try { AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit; } catch { }

            Log.Add(@"   |\_/|          |", Message.EContentType.Info);
            Log.Add(@"  >(o.O)<         | Zen " + System.Reflection.Assembly.GetCallingAssembly().GetName().Version, Message.EContentType.Info);
            Log.Add(@"  c(___)          |", Message.EContentType.Info);

            Log.Add("Cache             : " + (Cache == null ? "(none)" : Cache.ToString()), Message.EContentType.MoreInfo);
            Log.Add("Environment       : " + (Environment == null ? "(none)" : Environment.ToString()), Message.EContentType.MoreInfo);
            Log.Add("Log               : " + (Log == null ? "(none)" : Log.ToString()), Message.EContentType.MoreInfo);
            Log.Add("Encryption        : " + (Encryption == null ? "(none)" : Encryption.ToString()), Message.EContentType.MoreInfo);
            Log.Add("Authorization     : " + (Authorization == null ? "(none)" : Authorization.ToString()), Message.EContentType.MoreInfo);
            Log.Add("Global BundleType : " + (GlobalConnectionBundleType == null ? "(none)" : GlobalConnectionBundleType.ToString()), Message.EContentType.MoreInfo);
            Log.Add("Application       : " + Configuration.ApplicationAssemblyName, Message.EContentType.MoreInfo);
            Log.Add("App Location      : " + Configuration.BaseDirectory, Message.EContentType.MoreInfo);
            Log.Add("App Data          : " + Configuration.DataDirectory, Message.EContentType.MoreInfo);

            Log.Add("Stack status      : Operational", Message.EContentType.StartupSequence);

            //Post-initialization procedures

        }

        internal static IServiceCollection Services { get; }
        internal static ServiceProvider ServiceProvider { get; }

        public static ICacheProvider Cache => _cacheProvider.Value;
        public static IEnvironmentProvider Environment => _environmentProvider.Value;
        public static IEncryptionProvider Encryption => _IEncryptionProvider.Value;
        public static IAuthorizationProvider Authorization => _IAuthorizationProvider.Value;
        public static ILogProvider Log => _ILogProvider.Value;
        public static Type GlobalConnectionBundleType => ServiceProvider.GetService<Type>();

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e) { Events.End("Process Exit"); }

        private static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceProviderCfg)
        {
            IPackage package = null;

            try
            {
                // If a definition package is available use it; otherwise offer an empty package.
                package = (Management.GetClassesByInterface<IPackage>().FirstOrDefault() ?? typeof(DefaultSettingsPackage)).CreateInstance<IPackage>();

                serviceProviderCfg.AddSingleton(s => package.Log ?? Management.GetClassesByInterface<ILogProvider>().FirstOrDefault()?.CreateInstance<ILogProvider>());
                serviceProviderCfg.AddSingleton(s => package.Cache ?? Management.GetClassesByInterface<ICacheProvider>().FirstOrDefault()?.CreateInstance<ICacheProvider>());
                serviceProviderCfg.AddSingleton(s => package.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>().FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceProviderCfg.AddSingleton(s => package.Environment ?? Management.GetClassesByInterface<IEnvironmentProvider>().FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());
                serviceProviderCfg.AddSingleton(s => package.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>().FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceProviderCfg.AddSingleton(s => package.Authorization ?? Management.GetClassesByInterface<IAuthorizationProvider>().FirstOrDefault()?.CreateInstance<IAuthorizationProvider>());
                serviceProviderCfg.AddSingleton(s => package.GlobalConnectionBundleType ?? Management.GetClassesByInterface<ConnectionBundlePrimitive>().FirstOrDefault());
            } catch (Exception e)
            {
                //It's OK to ignore errors here.
            }

            return serviceProviderCfg;
        }

        // ReSharper disable InconsistentNaming
        private static readonly Lazy<ICacheProvider> _cacheProvider = new Lazy<ICacheProvider>(() => ServiceProvider.GetService<ICacheProvider>());
        private static readonly Lazy<IEnvironmentProvider> _environmentProvider = new Lazy<IEnvironmentProvider>(() => ServiceProvider.GetService<IEnvironmentProvider>());
        private static readonly Lazy<IEncryptionProvider> _IEncryptionProvider = new Lazy<IEncryptionProvider>(() => ServiceProvider.GetService<IEncryptionProvider>());
        private static readonly Lazy<IAuthorizationProvider> _IAuthorizationProvider = new Lazy<IAuthorizationProvider>(() => ServiceProvider.GetService<IAuthorizationProvider>());
        private static readonly Lazy<ILogProvider> _ILogProvider = new Lazy<ILogProvider>(() => ServiceProvider.GetService<ILogProvider>());
        // ReSharper restore InconsistentNaming
    }
}