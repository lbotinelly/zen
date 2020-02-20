using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Communication;
using Zen.App.Core.Application;
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
            serviceCollection.AddZenProvider<IEmailConfig>("Email Configuration");
            serviceCollection.AddZenProvider<IApplicationProvider>("Application Provider");
            serviceCollection.AddZenProvider<IZenOrchestrator>("Orchestrator");

            //serviceCollection.AddZenProvider<IZenPreference>("Preferences Provider");
            serviceCollection.AddSingleton(s => IoC.GetClassesByInterface<IZenPreference>(false).FirstOrDefault()?.CreateInstance<IZenPreference>());

            return serviceCollection;
        }
    }
}