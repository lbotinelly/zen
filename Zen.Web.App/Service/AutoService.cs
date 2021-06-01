using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;

namespace Zen.Web.App.Service.Extensions
{
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }
    }
}
