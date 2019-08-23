using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Maintenance;
using Zen.Base.Module.Service;

namespace Zen.Base.Service.Extensions
{
    public static class Add
    {
        public static ZenBuilder AddZen(this IServiceCollection services, string defaultScheme) { return services.AddZen(o => o.DefaultScheme = defaultScheme); }

        public static ZenBuilder AddZen(this IServiceCollection services, Action<ZenOptions> configureOptions = null)
        {
            Instances.ServiceCollection = services;

            if (services == null) throw new ArgumentNullException(nameof(services));

            // services.AddZenIdentityProvider<User>();

            Current.State = Status.EState.Starting;

            var builder = new ZenBuilder(services);

            if (configureOptions != null) services.Configure(configureOptions);

            services.AddHostedService<Maintenance.MaintenanceService>();

            return builder;
        }
    }
}