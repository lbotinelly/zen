using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Module.Cloud.AWS.Service
{
    [Priority(Level = -99)]
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }
    }
}