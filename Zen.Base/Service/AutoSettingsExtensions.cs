using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Default;
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

            var configurationPackage = (IoC.GetClassesByInterface<IConfigurationPackage>(false).FirstOrDefault() ?? typeof(DefaultSettingsPackage)).CreateInstance<IConfigurationPackage>();

            serviceCollection.AddSingleton(configurationPackage);

            serviceCollection.AddSingleton(s => configurationPackage.Environment ?? IoC.GetClassesByInterface<IEnvironmentProvider>(false).FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());
            serviceCollection.AddSingleton(s => configurationPackage.Log ?? IoC.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>());

            // ICacheProvider may receive injection, so let's do this by type.
            serviceCollection.AddSingleton(typeof(ICacheProvider), configurationPackage.Cache != null ? configurationPackage.Cache.GetType() : IoC.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault());
            serviceCollection.AddSingleton(s => configurationPackage.Encryption ?? IoC.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
            serviceCollection.AddSingleton(s => configurationPackage.GlobalConnectionBundleType ?? IoC.GetClassesByInterface<ConnectionBundlePrimitive>().FirstOrDefault());

            return serviceCollection;
        }
    }
}