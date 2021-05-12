using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Web.OpenApi.Service.Extensions;

namespace Zen.Web.OpenApi.Service
{
    [Priority(Level = -97)]
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.AddOpenApi();
        }
    }
}