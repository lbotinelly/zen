using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Storage.Provider.Configuration;
using Zen.Storage.Provider.File;

namespace Zen.Storage.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddZenProvider<ConfigurationStorage>("Configuration Storage")
                .AddZenProvider<FileStoragePrimitive>("File Storage")
                ;

            return serviceCollection;
        }
    }
}