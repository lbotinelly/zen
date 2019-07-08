using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Identity.Extensions;
using Zen.Base.Module.Identity.Model;

namespace Zen.Base.Service
{
    public static class ZenAddExtensions
    {
        public static ZenBuilder AddZen(this IServiceCollection services, string defaultScheme)
        {
            Module.Service.Instances.ServiceCollection = services;

            return services.AddZen(o => o.DefaultScheme = defaultScheme);
        }

        public static ZenBuilder AddZen(this IServiceCollection services, Action<ZenOptions> configureOptions = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddZenIdentityProvider<User>();

            foreach (var item in Module.Service.Instances.AutoZenServices)
                item.Add(services);

            var builder = new ZenBuilder(services);

            if (configureOptions != null)
                services.Configure(configureOptions);

            return builder;
        }
    }
}