using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Provider.Google.Authentication;

namespace Zen.Provider.Google
{
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.Configure();
        }
    }
}