using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Module.Service;

namespace Zen.Base.Service {
    public class ZenBaseAutoService : IZenAutoAddService, IZenAutoUseService
    {
        #region Implementation of IZenAutoAddService

        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
        }

        #endregion

        #region Implementation of IZenAutoUseService

        public void Use(IApplicationBuilder app, IHostingEnvironment env = null)
        {

        }
        #endregion
    }
}