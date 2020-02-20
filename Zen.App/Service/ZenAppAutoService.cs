using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.App.Service
{
    [Priority(Level = -0)]
    public class ZenAppAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }
    }
}