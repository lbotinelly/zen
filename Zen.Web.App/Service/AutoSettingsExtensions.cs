using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.App.Common;
using Zen.Web.App.Communication.Push;

namespace Zen.Web.App.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddZenProvider<IZenWebOrchestrator>("Zen Web Orchestrator");
            serviceCollection.AddZenProvider<PushDispatcherPrimitive>("Push Dispatcher");

            return serviceCollection;
        }
    }
}
