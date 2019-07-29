using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Provider;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.App.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            var probe = Resolution.GetClassesByInterface<IZenOrchestrator>(false).FirstOrDefault()?.CreateInstance<IZenOrchestrator>();
            serviceCollection.AddSingleton(s => probe);
            return serviceCollection;
        }
    }
}