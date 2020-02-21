using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.Cloud.Provider;

namespace Zen.Cloud.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            var targetProvider = IoC.GetClassesByInterface<ICloudProvider>(false).FirstOrDefault()?.CreateInstance<ICloudProvider>();

            if (targetProvider == null) return serviceCollection;

            serviceCollection.AddSingleton(s => targetProvider);
            Events.AddLog("Cloud Provider", targetProvider?.ToString());

            return serviceCollection;
            
        }
    }
}