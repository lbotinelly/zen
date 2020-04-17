using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Module.Cache.Memory.Service.Extensions;

namespace Zen.Module.Cache.Memory.Service
{
    [Priority(Level = -98)]
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.AddModule();
        }
    }
}