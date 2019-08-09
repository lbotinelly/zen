using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

            app.UseDefaultFiles();

            app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")), RequestPath = "" });


            app
                .UseHttpsRedirection()
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