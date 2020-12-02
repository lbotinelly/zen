using Microsoft.Extensions.DependencyInjection;
using Zen.Base;

namespace Zen.Web.GraphQL.Service.Extensions
{
    public static class Add
    {
        public static void AddGraphQl(this IServiceCollection services)
        {
            services.ResolveSettingsPackage();
            services.Configure<Configuration.Options>(options =>
                options.GetSettings<Configuration.IOptions, Configuration.Options>("GraphQL"));
        }
    }
}