using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Identity;
using Zen.Base.Identity.Extensions;
using Zen.Base.Identity.Model;

namespace Zen.Base.Startup
{
    public static class ZenAddExtensions
    {
        public static ZenBuilder AddZen(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));


            services.AddZenIdentityProvider<ZenUser>(mongo => { });


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