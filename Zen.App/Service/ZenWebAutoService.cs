using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;

namespace Zen.App.Service
{
    public class ZenAppAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }
    }
}