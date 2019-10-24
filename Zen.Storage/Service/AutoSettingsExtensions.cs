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
                .AddZenProvider<ZenConfigurationStorage>("Configuration Storage")
                .AddZenProvider<ZenFileStoragePrimitive>("File Storage")
                //.AddZenProvider<IZenFileDescriptor>("File Descriptor")
                ;

            return serviceCollection;
        }
    }
}