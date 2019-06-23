using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace Zen.Base.Startup
{
    public static class ZenServiceCollectionExtensions
    {
        public static ZenBuilder AddZen(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
       
            return new ZenBuilder(services);
        }

        public static ZenBuilder AddZen(this IServiceCollection services, string defaultScheme) => services.AddZen(o => o.DefaultScheme = defaultScheme);

        public static ZenBuilder AddZen(this IServiceCollection services, Action<ZenOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var builder = services.AddZen();
            services.Configure(configureOptions);
            return builder;
        }
    }
}
