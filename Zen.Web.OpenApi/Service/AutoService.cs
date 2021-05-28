using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Web.OpenApi.Service
{
    [Priority(Level = -97)]
    public class AutoService : IZenAutoAddService, IZenAutoUseService
    {
        public void Add(IServiceCollection services)
        {
            services.ResolveSettingsPackage();
            services.AddOpenApiDocument();
            services.Configure<Configuration.Options>(options => options.GetSettings<Configuration.IOptions, Configuration.Options>("OpenAPI"));
        }

        public void Use(IApplicationBuilder app, IHostEnvironment env = null)
        {
            app.UseOpenApi(); // Serves the registered OpenAPI/Swagger documents by default on `/swagger/{documentName}/swagger.json`
            app.UseSwaggerUi3();
            app.UseReDoc();
        }
    }
}