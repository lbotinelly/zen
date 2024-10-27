using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
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
            //services.AddOpenApiDocument();
            services.AddOpenApiDocument(settings =>
            {
                var app =  System.Reflection.Assembly.GetEntryAssembly();

                var appName = app.CustomAttributes.FirstOrDefault(i => i.AttributeType == typeof(System.Reflection.AssemblyProductAttribute))?.ConstructorArguments.FirstOrDefault().Value?.ToString() ??
                app.GetName().Name;

                var appDescription = app.CustomAttributes.FirstOrDefault(i => i.AttributeType == typeof(System.Reflection.AssemblyDescriptionAttribute))?.ConstructorArguments.FirstOrDefault().Value?.ToString();

                settings.Title = appName;
                settings.Description = appDescription;
            });
            services.Configure<Configuration.Options>(options => options.GetSettings<Configuration.IOptions, Configuration.Options>("OpenAPI"));
        }

        public void Use(IApplicationBuilder app, IHostEnvironment env = null)
        {
            app.UseOpenApi(); // Serves the registered OpenAPI/Swagger documents by default on `/swagger/{documentName}/swagger.json`

            if (Base.Host.IsDevelopment)
            {
                app.UseSwaggerUi3();
                app.UseReDoc();
            }
        }

        public void Use(IHost app, IHostEnvironment env = null)
        {
            throw new System.NotImplementedException();
        }
    }
}