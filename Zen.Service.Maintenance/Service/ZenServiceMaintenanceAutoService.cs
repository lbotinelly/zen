using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Service.Maintenance.Service
{
    [Priority(Level = -97)]
    public class ZenServiceMaintenanceAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.AddHostedService<MaintenanceService>();
        }
    }
}