using Microsoft.Extensions.DependencyInjection;
using Zen.Base;

namespace Zen.Web.OpenApi.Service.Extensions
{
    public static class Add
    {
        public static void AddOpenApi(this IServiceCollection services)
        {
            services.ResolveSettingsPackage();

            services.Configure<Configuration.Options>(options =>
                options.GetSettings<Configuration.IOptions, Configuration.Options>("OpenAPI"));
        }
    }
}