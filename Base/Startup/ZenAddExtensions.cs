using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Identity.Extensions;
using Zen.Base.Identity.Model;

namespace Zen.Base.Startup
{
    public static class ZenAddExtensions
    {
        public static ZenBuilder AddZen(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddZenIdentityProvider<ZenUser>();

            return new ZenBuilder(services);
        }

        public static ZenBuilder AddZen(this IServiceCollection services, string defaultScheme) { return services.AddZen(o => o.DefaultScheme = defaultScheme); }

        public static ZenBuilder AddZen(this IServiceCollection services, Action<ZenOptions> configureOptions = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var builder = services.AddZen();

            services.Configure(configureOptions);
            return builder;
        }
    }
}