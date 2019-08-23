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

            // If a definition package is available use it; otherwise offer an empty package.

            var configurationPackage = (Resolution.GetClassesByInterface<IConfigurationPackage>(false).FirstOrDefault() ?? typeof(DefaultSettingsPackage)).CreateInstance<IConfigurationPackage>();

            serviceCollection.AddSingleton(configurationPackage);

            //var a = configurationPackage.Log ?? Resolution.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>();
            //var b = configurationPackage.Cache ?? Resolution.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()?.CreateInstance<ICacheProvider>();

            serviceCollection.AddSingleton(s => configurationPackage.Log ?? Resolution.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>());
            serviceCollection.AddSingleton(s => configurationPackage.Cache ?? Resolution.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()?.CreateInstance<ICacheProvider>());
            serviceCollection.AddSingleton(s => configurationPackage.Encryption ?? Resolution.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
            serviceCollection.AddSingleton(s => configurationPackage.Environment ?? Resolution.GetClassesByInterface<IEnvironmentProvider>(false).FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());
            serviceCollection.AddSingleton(s => configurationPackage.GlobalConnectionBundleType ?? Resolution.GetClassesByInterface<ConnectionBundlePrimitive>().FirstOrDefault());

            return serviceCollection;
        }
    }
}