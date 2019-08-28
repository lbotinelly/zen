using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Cloud.Service
{
    [Priority(Level = -97)]
    public class ZenStorageAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }
    }
}