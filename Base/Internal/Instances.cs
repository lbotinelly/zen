using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Assembly;
using Zen.Base.Common;
using Zen.Base.Identity;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Default;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Log;

namespace Zen.Base.Internal
{
    internal static class Instances
    {
        internal class ServiceDataBag
        {
            public DateTime StartTimeStamp { get; set; }
            public DateTime? EndTimeStamp { get; set; }
            public TimeSpan UpTime => (EndTimeStamp ?? DateTime.Now) - StartTimeStamp;
        }

        internal static ServiceDataBag ServiceData = new ServiceDataBag();


        static Instances()
        {
            Services = new ServiceCollection().ResolveSettingsPackage();
            ServiceProvider = Services.BuildServiceProvider();
        }


        internal static IServiceCollection Services { get; set; }
        internal static ServiceProvider ServiceProvider { get; set; }

        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            IPackage package = null;

            try
            {
                // If a definition package is available use it; otherwise offer an empty package.
                package = (Management.GetClassesByInterface<IPackage>(false).FirstOrDefault() ??
                           typeof(DefaultSettingsPackage)).CreateInstance<IPackage>();

                serviceCollection.AddSingleton(s =>
                    package.Log ?? Management.GetClassesByInterface<ILogProvider>(false).FirstOrDefault()
                        ?.CreateInstance<ILogProvider>());

                serviceCollection.AddSingleton(s =>
                    package.Cache ?? Management.GetClassesByInterface<ICacheProvider>(false).FirstOrDefault()
                        ?.CreateInstance<ICacheProvider>());

                serviceCollection.AddSingleton(s =>
                    package.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()
                        ?.CreateInstance<IEncryptionProvider>());

                serviceCollection.AddSingleton(s =>
                    package.Environment ?? Management.GetClassesByInterface<IEnvironmentProvider>(false)
                        .FirstOrDefault()?.CreateInstance<IEnvironmentProvider>());

                serviceCollection.AddSingleton(s =>
                    package.Encryption ?? Management.GetClassesByInterface<IEncryptionProvider>(false).FirstOrDefault()
                        ?.CreateInstance<IEncryptionProvider>());

                serviceCollection.AddSingleton(s =>
                    package.Authorization ?? Management.GetClassesByInterface<IAuthorizationProvider>(false)
                        .FirstOrDefault()?.CreateInstance<IAuthorizationProvider>());

                serviceCollection.AddSingleton(s =>
                    package.GlobalConnectionBundleType ?? Management.GetClassesByInterface<ConnectionBundlePrimitive>()
                        .FirstOrDefault());
            }
            catch (Exception e)
            {
                //It's OK to ignore errors here.
            }

            return serviceCollection;
        }
    }
}