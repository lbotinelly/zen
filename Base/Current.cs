using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
            Services = new ServiceCollection().ResolveSettingsPackage();

            ServiceProvider = Services.BuildServiceProvider();

            var providers = Services.Where(i => typeof(IZenProvider).IsAssignableFrom(i.ServiceType)).ToList();

            foreach (var zenService in providers) ((IZenProvider) ServiceProvider.GetService(zenService.ServiceType)).Initialize();

            Events.Start();

            try { AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit; } catch { }

            Log.Info(@"Zen " + System.Reflection.Assembly.GetCallingAssembly().GetName().Version);
            Log.Debug("________________________________________________________________________________");
            Log.Debug("");
            Log.Debug("            Cache : " + (Cache == null ? "(none)" : Cache.ToString()));
            Log.Debug("      Environment : " + (Environment == null ? "(none)" : Environment.ToString()));
            Log.Debug("              Log : " + (Log == null ? "(none)" : Log.ToString()));
            Log.Debug("       Encryption : " + (Encryption == null ? "(none)" : Encryption.ToString()));
            Log.Debug("    Authorization : " + (Authorization == null ? "(none)" : Authorization.ToString()));
            Log.Debug("Global BundleType : " + (GlobalConnectionBundleType == null ? "(none)" : GlobalConnectionBundleType.ToString()));
            Log.Debug("      Application : " + Configuration.ApplicationAssemblyName);
            Log.Debug("     App Location : " + Configuration.BaseDirectory);
            Log.Debug("         App Data : " + Configuration.DataDirectory);
            Log.Debug("________________________________________________________________________________");

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
                package = (Management.GetClassesByInterface<IPackage>(false).FirstOrDefault() ?? typeof(DefaultSettingsPackage)).CreateInstance<IPackage>();

                serviceProviderCfg.AddSingleton(s => package.Log ?? Management.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>());
                serviceProviderCfg.AddSingleton(s => package.Cache ?? Management.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()?.CreateInstance<ICacheProvider>());
                serviceProviderCfg.AddSingleton(s => package.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceProviderCfg.AddSingleton(s => package.Environment ?? Management.GetClassesByInterface<IEnvironmentProvider>(false).FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());
                serviceProviderCfg.AddSingleton(s => package.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceProviderCfg.AddSingleton(s => package.Authorization ?? Management.GetClassesByInterface<IAuthorizationProvider>(false).FirstOrDefault()?.CreateInstance<IAuthorizationProvider>());
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