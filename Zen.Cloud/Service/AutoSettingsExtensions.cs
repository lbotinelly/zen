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
            var probe = IoC.GetClassesByInterface<ICloudProvider>(false).FirstOrDefault()?.CreateInstance<ICloudProvider>();

            if (probe == null) return serviceCollection;

            serviceCollection.AddSingleton(s => probe);
            Events.AddLog("Cloud Provider", probe?.ToString());

            return serviceCollection;
            
        }
    }
}