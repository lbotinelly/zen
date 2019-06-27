using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Internal;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Identity;
using Zen.Base.Module.Identity.Model;
using Zen.Base.Module.Log;

namespace Zen.Base
{
    public static class Current
    {
        static Current()
        {
            Events.InitializeServices();
            Events.Start();
        }

        public static ICacheProvider Cache => _cacheProvider.Value;
        public static IEnvironmentProvider Environment => _environmentProvider.Value;
        public static IEncryptionProvider Encryption => _IEncryptionProvider.Value;
        public static IAuthorizationProvider Authorization => _IAuthorizationProvider.Value;
        public static ILogProvider Log => _ILogProvider.Value;
        public static Type GlobalConnectionBundleType => Instances.ServiceProvider.GetService<Type>();
        public static User Person { get; set; }

        // ReSharper disable InconsistentNaming
        private static readonly Lazy<ICacheProvider> _cacheProvider =
            new Lazy<ICacheProvider>(() => Instances.ServiceProvider.GetService<ICacheProvider>(), true);

        private static readonly Lazy<IEnvironmentProvider> _environmentProvider =
            new Lazy<IEnvironmentProvider>(() => Instances.ServiceProvider.GetService<IEnvironmentProvider>(), true);

        private static readonly Lazy<IEncryptionProvider> _IEncryptionProvider =
            new Lazy<IEncryptionProvider>(() => Instances.ServiceProvider.GetService<IEncryptionProvider>(), true);

        private static readonly Lazy<IAuthorizationProvider> _IAuthorizationProvider =
            new Lazy<IAuthorizationProvider>(() => Instances.ServiceProvider.GetService<IAuthorizationProvider>(), true);

        private static readonly Lazy<ILogProvider> _ILogProvider =
            new Lazy<ILogProvider>(() => Instances.ServiceProvider.GetService<ILogProvider>(), true);
        // ReSharper restore InconsistentNaming
    }
}