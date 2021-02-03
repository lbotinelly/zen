using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Base.Service
{
    [Priority(Level = 1)]
    public class ZenBaseAutoService : IZenAutoAddService, IZenAutoUseService
    {
        public void Add(IServiceCollection services) => services.ResolveSettingsPackage();
        public void Use(IApplicationBuilder app, IHostEnvironment env = null) { }
    }
}