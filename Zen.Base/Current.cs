using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

namespace Zen.Base
{
    public static class Current
    {
        static Current() { Events.Start(); }
        public static Status.EState State { get; internal set; } = Status.State;
        public static ICacheProvider Cache => _cacheProvider.Value;
        public static IEnvironmentProvider Environment => _environmentProvider.Value;
        public static IEncryptionProvider Encryption => _IEncryptionProvider.Value;
        public static ILogProvider Log => _ILogProvider.Value;

        // ReSharper disable InconsistentNaming
        private static readonly Lazy<ICacheProvider> _cacheProvider = new Lazy<ICacheProvider>(() => Instances.ServiceProvider.GetService<ICacheProvider>(), true);
        private static readonly Lazy<IEnvironmentProvider> _environmentProvider = new Lazy<IEnvironmentProvider>(() =>
        {
            var probe = Instances.ServiceProvider.GetService<IEnvironmentProvider>();
            Events.AddLog("Environment", probe.Current?.ToString());
            return probe;
        }, true);
        private static readonly Lazy<IEncryptionProvider> _IEncryptionProvider = new Lazy<IEncryptionProvider>(() => Instances.ServiceProvider.GetService<IEncryptionProvider>(), true);
        private static readonly Lazy<ILogProvider> _ILogProvider = new Lazy<ILogProvider>(valueFactory: () => Instances.ServiceProvider.GetService<ILogProvider>(), true);
        // ReSharper restore InconsistentNaming
    }
}