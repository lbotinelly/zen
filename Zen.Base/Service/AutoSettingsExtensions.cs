using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

namespace Zen.Base.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions();

            IEnumerable<IConfigurationPackage> configurationPackages = IoC.GetClassesByInterface<ConfigurationPackagePrimitive>(false).CreateInstances<IConfigurationPackage>().ToList();

            serviceCollection.AddSingletonProvider<ILogProvider, LogOptions>(configurationPackages, "Log");
            serviceCollection.AddSingletonProvider<IEnvironmentProvider, EnvironmentOptions>(configurationPackages, "Environment");
            serviceCollection.AddSingletonProvider<ICacheProvider, CacheOptions>(configurationPackages, "Cache");
            serviceCollection.AddSingletonProvider<IEncryptionProvider, EncryptionOptions>(configurationPackages, "Encryption");
            serviceCollection.AddSingletonProvider<IConnectionBundleProvider, ConnectionBundleOptions>(configurationPackages, "ConnectionBundle");

            return serviceCollection;
        }
    }
}