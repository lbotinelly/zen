using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Default;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Identity;
using Zen.Base.Module.Log;
using Zen.Base.Service;

namespace Zen.Base.Module.Service
{
    public static class Instances
    {
        internal static ServiceDataBag ServiceData = new ServiceDataBag();

        static Instances()
        {

            Instances.AutoZenServices = Resolution.GetInstances<IZenAutoService>();


            ServiceCollection = new ServiceCollection()
                .ResolveSettingsPackage();

            ServiceProvider = ServiceCollection
                .BuildServiceProvider();
        }

        public static IServiceCollection ServiceCollection { get; internal set; }
        public static ServiceProvider ServiceProvider { get; internal set; }
        public static IApplicationBuilder ApplicationBuilder { get; internal set; }
        public static List<IZenAutoService> AutoZenServices { get; internal set; }

        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            IConfigurationPackage configurationPackage = null;

            try
            {
                // If a definition package is available use it; otherwise offer an empty package.

                configurationPackage = (Resolution.GetClassesByInterface<IConfigurationPackage>(false).FirstOrDefault() ?? typeof(DefaultSettingsPackage)).CreateInstance<IConfigurationPackage>();

                serviceCollection.AddSingleton(configurationPackage);

                var a = configurationPackage.Log ?? Resolution.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>();
                var b = configurationPackage.Cache ?? Resolution.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()?.CreateInstance<ICacheProvider>();

                serviceCollection.AddSingleton(s => configurationPackage.Log ?? Resolution.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()?.CreateInstance<ILogProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Cache ?? Resolution.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()?.CreateInstance<ICacheProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Encryption ?? Resolution.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Environment ?? Resolution.GetClassesByInterface<IEnvironmentProvider>(false).FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Encryption ?? Resolution.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()?.CreateInstance<IEncryptionProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.Authorization ?? Resolution.GetClassesByInterface<IAuthorizationProvider>(false).FirstOrDefault()?.CreateInstance<IAuthorizationProvider>());
                serviceCollection.AddSingleton(s => configurationPackage.GlobalConnectionBundleType ?? Resolution.GetClassesByInterface<ConnectionBundlePrimitive>().FirstOrDefault());
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