using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Provider.GitHub.Authentication;

namespace Zen.Provider.GitHub
{
    public class GitHubAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.AddAuthenticationProvider();
        }
    }
}