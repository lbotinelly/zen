using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Web.SelfHost.Service.Extensions;

namespace Zen.Web.SelfHost.Service
{
    [Priority(Level = -97)]
    public class AutoService : IZenAutoAddService, IZenAutoUseService
    {
        public void Add(IServiceCollection services)
        {
            services.AddZenWebSelfHost();
        }

        public void Use(IApplicationBuilder app, IHostEnvironment env = null)
        {
            app.UseZenWebSelfHost();
        }
    }
}