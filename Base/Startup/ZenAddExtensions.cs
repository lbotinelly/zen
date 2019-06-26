using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Identity.Extensions;
using Zen.Base.Module.Identity.Model;

namespace Zen.Base.Startup
{
    public static class ZenAddExtensions
    {
        public static ZenBuilder AddZen(this IServiceCollection services, string defaultScheme) { return services.AddZen(o => o.DefaultScheme = defaultScheme); }

        public static ZenBuilder AddZen(this IServiceCollection services, Action<ZenOptions> configureOptions = null)
        {

            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddZenIdentityProvider<ZenUser>();


            var builder =  new ZenBuilder(services);

            if (configureOptions != null)
                services.Configure(configureOptions);

            return builder;
        }
    }
}