using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.DependencyInjection;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Default;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Identity;
using Zen.Base.Module.Log;

namespace Zen.Base.Internal
{
    public static class Instances
    {
        internal static ServiceDataBag ServiceData = new ServiceDataBag();

        static Instances()
        {
            ServiceCollection = new ServiceCollection()
                .ResolveSettingsPackage();

            ServiceProvider = ServiceCollection
                .BuildServiceProvider();
        }

        public static IServiceCollection ServiceCollection { get; set; }
        public static ServiceProvider ServiceProvider { get; set; }

        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            IConfigurationPackage configurationPackage = null;

            try
            {
                // If a definition package is available use it; otherwise offer an empty package.

                configurationPackage = (Management.GetClassesByInterface<IConfigurationPackage>(false).FirstOrDefault() ?? typeof(DefaultSettingsPackage)).CreateInstance<IConfigurationPackage>();

                serviceCollection.AddSingleton<IConfigurationPackage>(configurationPackage);

                serviceCollection.AddSingleton(s => configurationPackage.Log ?? Management.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Cache ?? Management.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()?.CreateInstance<ICacheProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Environment ?? Management.GetClassesByInterface<IEnvironmentProvider>(false).FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Authorization ?? Management.GetClassesByInterface<IAuthorizationProvider>(false).FirstOrDefault()?.CreateInstance<IAuthorizationProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.GlobalConnectionBundleType ?? Management.GetClassesByInterface<ConnectionBundlePrimitive>().FirstOrDefault());
            }
            catch (Exception e)
            {
                //It's OK to ignore errors here.
            }

            return serviceCollection;
        }

        internal class ServiceDataBag
        {
            public DateTime StartTimeStamp { get; set; }
            public DateTime? EndTimeStamp { get; set; }
            public TimeSpan UpTime => (EndTimeStamp ?? DateTime.Now) - StartTimeStamp;
        }

    }
}