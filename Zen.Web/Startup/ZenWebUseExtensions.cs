using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zen.Base.Startup;

namespace Zen.Web.Startup
{
    public static class ZenWebUseExtensions
    {
        public static void UseZenWeb(this IApplicationBuilder app, Action<IZenWebBuilder> configuration, IHostingEnvironment env = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var optionsProvider = app.ApplicationServices.GetService<IOptions<ZenWebOptions>>();

            var options = new ZenWebOptions(optionsProvider.Value);

            var builder = new ZenWebBuilder(app, options);

            app.UseZen()
                .UseAuthentication()
                .UseHttpsRedirection()
                .UseMvc();

                //.UseStaticFiles();
                //.UseSpaStaticFiles();

            //app.UseSpa(spa =>
            //{
            //    // To learn more about options for serving an Angular SPA from ASP.NET Core,
            //    // see https://go.microsoft.com/fwlink/?linkid=864501

            //    spa.Options.SourcePath = "ClientApp";

            //    if (env?.IsDevelopment() == true) spa.UseAngularCliServer("start");
            //});

            configuration.Invoke(builder);
        }
    }
}