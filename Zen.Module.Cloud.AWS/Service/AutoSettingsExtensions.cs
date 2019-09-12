using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Module.Cloud.AWS.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDefaultAWSOptions(Base.Configuration.Options.GetAWSOptions());
            serviceCollection.AddAWSService<IAmazonSimpleNotificationService>();

            return serviceCollection;
        }
    }
}