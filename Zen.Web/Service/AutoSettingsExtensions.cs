using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.Common;
using Zen.Web.Communication.Push;

namespace Zen.Web.Service {
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