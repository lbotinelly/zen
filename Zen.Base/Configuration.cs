using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Base
{
    public static class Configuration
    {
        static Configuration()
        {
            var configurationBuilder =
                    new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("zen.json", true, true)
                        .AddEnvironmentVariables("ZEN_")
                ;

            Options = configurationBuilder.Build();
        }

        public static IConfigurationRoot Options { get; }

        public static T SetOptions<T>(IConfigurationPackage package, string sectionCode) where T : class =>
            package?.Provider[typeof(T)] != null
                ? (T) package.Provider[typeof(T)]
                : IoC.GetClassesByInterface<T>().FirstOrDefault()?.CreateInstance<T>() ??
                  Options.GetSection(sectionCode).Get<T>();

        public static void SetOptions<T>(this IServiceCollection serviceCollection, IConfigurationPackage package, string sectionCode) where T : class => serviceCollection.Configure<T>(c => SetOptions<T>(package, sectionCode));

        public static void AddSingletonProvider<T, TU>(this IServiceCollection serviceCollection, IEnumerable<IConfigurationPackage> packages, string sectionCode) where T : class where TU : class => serviceCollection.AddSingletonProvider<T, TU>(packages.FirstOrDefault(i => i.Provider.ContainsKey(typeof(T))), sectionCode);

        public static void AddSingletonProvider<T, TU>(this IServiceCollection serviceCollection, IConfigurationPackage package, string sectionCode) where T : class where TU : class
        {
            var targetType = typeof(T);

            var targetProvider = package?.Provider?.ContainsKey(targetType) == true ? (Type) package.Provider[targetType] : IoC.GetClassesByInterface<T>().FirstOrDefault();

            if (targetProvider != null)
            {
                serviceCollection.AddSingleton(typeof(T), targetProvider);
                Status.Providers.Add(targetType);
            }

            serviceCollection.SetOptions<TU>(package, sectionCode);
        }

        public static TInterface GetSettings<TInterface, TConcrete>(this TConcrete options, string sectionKey) where TInterface : class where TConcrete : TInterface
        {
            var configOptions = (IoC.GetClassesByInterface<TInterface>(true).FirstOrDefault()?.ToInstance<TInterface>() ?? // Any concrete defined type?
                                 Options.GetSection(sectionKey).Get<TConcrete>()) ?? // Any described option in Config?
                                IoC.GetClassesByInterface<TInterface>().FirstOrDefault()?.ToInstance<TInterface>(); // Load from Fallback.

            configOptions.CopyProperties(options);

            return configOptions;
        }
    }
}