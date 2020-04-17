using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.App.Service
{
    [Priority(Level = 0)]
    public class ZenAppAutoService : IZenAutoAddService, IZenAutoUseService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }

        public void Use(IApplicationBuilder app, IHostEnvironment env = null)
        {
        }
    }
}