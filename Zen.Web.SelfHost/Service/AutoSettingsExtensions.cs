using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.SelfHost.Common;

namespace Zen.Web.SelfHost.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddZenProvider<ISelfHostOrchestrator>("Self-Host Orchestrator");
            return serviceCollection;
        }
    }
}