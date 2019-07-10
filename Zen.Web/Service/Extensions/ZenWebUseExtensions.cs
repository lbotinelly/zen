using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
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


            app
                .UseAuthentication()
                .UseHttpsRedirection()
                .UseMvc();

            if (options.UseSpa)
            {
                app.UseStaticFiles();

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