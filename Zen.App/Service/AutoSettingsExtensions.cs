using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Communication;
using Zen.App.Orchestrator;
using Zen.App.Provider;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.App.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddZenProvider<IZenOrchestrator>("Orchestrator");
            serviceCollection.AddZenProvider<IZenEmailConfig>("Email Configuration");

            //serviceCollection.AddZenProvider<IZenPreference>("Preferences Provider");
            serviceCollection.AddSingleton(s => Resolution.GetClassesByInterface<IZenPreference>(false).FirstOrDefault()?.CreateInstance<IZenPreference>());

            return serviceCollection;
        }
    }
}