using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Provider.Google.Authentication;

namespace Zen.Provider.Google.Service
{
    public class GoogleAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.AddAuthenticationProvider();
        }
    }
}