using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.Storage.Provider.Configuration;

namespace Zen.Storage.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            var probe = Resolution.GetClassesByInterface<ZenConfigurationStorage>(false).FirstOrDefault()?.CreateInstance<ZenConfigurationStorage>();
            serviceCollection.AddSingleton(s => probe);

            Events.AddLog("Configuration Storage", probe?.ToString());

            return serviceCollection;
        }
    }
}