using Microsoft.Extensions.DependencyInjection;

namespace Zen.Web.Service {
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {

            return serviceCollection;
        }
    }
}