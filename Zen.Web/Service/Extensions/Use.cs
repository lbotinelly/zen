using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zen.Base;

namespace Zen.Web.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWeb(this IApplicationBuilder app, Action<IZenWebBuilder> configuration = null, IHostingEnvironment env = null)
        {
            configuration = configuration ?? (x => { });

            var optionsProvider = app.ApplicationServices.GetService<IOptions<ZenWebOptions>>();

            var options = new ZenWebOptions(optionsProvider.Value);

            var builder = new ZenWebBuilder(app, options);

            var useAppCodeAsRoutePrefix = Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true;

            if (!useAppCodeAsRoutePrefix)
            {
                // Default behavior: nothing to see here.

                app.UseDefaultFiles();
                app.UseStaticFiles();
            }
            else
            {
                // The App code will be used as prefix for all requests, so let's move the root:

                var rootPrefix = "/" + App.Current.Configuration.Code.ToLower(); // e.g. "/appcode"

                Events.AddLog("Web.RootPrefix", rootPrefix);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"); // We're still using the default wwwroot folder

                var fileProvider = new PhysicalFileProvider(path);

                var fOptions = new DefaultFilesOptions
                {
                    FileProvider = fileProvider,
                    RequestPath = rootPrefix
                };

                app.UseDefaultFiles(fOptions); // This will allow default (index.html, etc.) requests on the new mapping

                app.UseStaticFiles(new StaticFileOptions {FileProvider = fileProvider, RequestPath = rootPrefix});

                app.UseRouter(r =>
                {
                    r.MapGet("", context =>
                    {
                        context.Response.Redirect("." + rootPrefix, false);
                        return Task.FromResult(0);
                    });
                });
            }

            app
                //.UseHttpsRedirection()
                .UseMvc();

            if (options.UseSpa)
            {
                // app.UseSpaStaticFiles();

                //app.UseSpa(spa =>
                //{
                //    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                //    // see https://go.microsoft.com/fwlink/?linkid=864501
                //    spa.Options.SourcePath = "ClientApp";
                //    //if (env?.IsDevelopment() == true)
                //    spa.UseAngularCliServer("start");
                //});
            }

            configuration.Invoke(builder);
        }
    }
}