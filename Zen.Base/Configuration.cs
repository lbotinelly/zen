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

        public static T SetOptions<T>(IConfigurationPackage package, string sectionCode) where T : class
        {
            if (package?.Provider[typeof(T)] != null) return (T)package.Provider[typeof(T)];
            return IoC.GetClassesByInterface<T>(false).FirstOrDefault()?.CreateInstance<T>() ?? Options.GetSection(sectionCode).Get<T>();
        }

        public static void SetOptions<T>(this IServiceCollection serviceCollection, IConfigurationPackage package, string sectionCode) where T : class
        {
            serviceCollection.Configure<T>(c => SetOptions<T>(package, sectionCode));
        }

        public static void AddSingletonProvider<T, TU>(this IServiceCollection serviceCollection, IEnumerable<IConfigurationPackage> packages, string sectionCode) where T : class where TU : class
        {
            var targetPackage = packages.FirstOrDefault(i => i.Provider.ContainsKey(typeof(T)));

            serviceCollection.AddSingletonProvider<T, TU>(targetPackage, sectionCode);

        }


        public static void AddSingletonProvider<T, TU>(this IServiceCollection serviceCollection, IConfigurationPackage package, string sectionCode) where T : class where TU : class
        {
            var targetProvider = package?.Provider?.ContainsKey(typeof(T)) == true ? (Type)package.Provider[typeof(T)] : IoC.GetClassesByInterface<T>(false).FirstOrDefault();

            if (targetProvider != null) serviceCollection.AddSingleton(typeof(T), targetProvider);

            serviceCollection.SetOptions<TU>(package, sectionCode);
        }
    }
}