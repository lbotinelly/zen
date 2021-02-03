using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Web.GraphQL.Service.Extensions;

namespace Zen.Web.GraphQL.Service
{
    [Priority(Level = -97)]
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.AddGraphQl();
        }
    }
}