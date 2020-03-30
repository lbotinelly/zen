using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;
using Zen.Provider.Microsoft.Authentication;

namespace Zen.Provider.Microsoft.Service
{
    [Priority(Level = -99)]
    public class MicrosoftAutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }
    }
}