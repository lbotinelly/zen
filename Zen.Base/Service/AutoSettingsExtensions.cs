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

            var configurationPackage = IoC.GetClassesByInterface<ConfigurationPackagePrimitive>(false).FirstOrDefault().CreateInstance<IConfigurationPackage>();
            serviceCollection.AddSingleton(configurationPackage);

            serviceCollection.AddSingletonProvider<ILogProvider, LogOptions>(configurationPackage, "Log");
            serviceCollection.AddSingletonProvider<IEnvironmentProvider, EnvironmentOptions>(configurationPackage, "Environment");
            serviceCollection.AddSingletonProvider<ICacheProvider, CacheOptions>(configurationPackage, "Cache");
            serviceCollection.AddSingletonProvider<IEncryptionProvider, EncryptionOptions>(configurationPackage, "Encryption");
            serviceCollection.AddSingletonProvider<IConnectionBundleProvider, ConnectionBundleOptions>(configurationPackage, "ConnectionBundle");

            return serviceCollection;
        }
    }
}