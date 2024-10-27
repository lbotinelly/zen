using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.OpenApi.Common;

namespace Zen.Web.OpenApi.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddZenProvider<IOpenApiProcessor>("OpenAPI Processor");
            return serviceCollection;
        }
    }
}