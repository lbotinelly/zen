using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.Auth.Provider;

namespace Zen.Web.Auth.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddZenProvider<IAuthPrimitive>("Authorization");

            return serviceCollection;
        }
    }
}