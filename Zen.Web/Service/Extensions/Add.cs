using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;

namespace Zen.Web.Service.Extensions
{
    public static class Add
    {
        public static ZenWebBuilder AddZenWeb(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.ResolveSettingsPackage();
            services.Configure<Configuration.Options>(options => options.GetSettings<Configuration.IOptions, Configuration.Options>("Web"));

            var builder = new ZenWebBuilder(services);

            return builder;
        }
    }
}