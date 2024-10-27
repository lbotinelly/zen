using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.GraphQL.Common;

namespace Zen.Web.GraphQL.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddZenProvider<IGraphQlProcessor>("GraphQL Processor");
            return serviceCollection;
        }
    }
}