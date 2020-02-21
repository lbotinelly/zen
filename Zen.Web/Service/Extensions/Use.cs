using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Web.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWeb(this IApplicationBuilder app, Action<IZenWebBuilder> configuration = null, IHostEnvironment env = null)
        {
            configuration = configuration ?? (x => { });

            var optionsProvider = app.ApplicationServices.GetService<IOptions<ZenWebOptions>>();

            var options = new ZenWebOptions(optionsProvider.Value);

            var builder = new ZenWebBuilder(app, options);

            var appCode = App.Current.Configuration.Code.ToLower();
            var usePrefix = Current.Configuration?.RoutePrefix != null || Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true;
            var prefix = Current.Configuration?.RoutePrefix ?? (Current.Configuration?.Behavior?.UseAppCodeAsRoutePrefix == true ? appCode : null);

            if (!usePrefix)
            {
                // Default behavior: nothing to see here.

                app.UseDefaultFiles();
                app.UseStaticFiles();
            }
            else
            {
                // The App code will be used as prefix for all requests, so let's move the root:

                var rootPrefix = "/" + prefix; // e.g. "/appcode"

                Base.Host.Variables[Host.Keys.WebRootPrefix] = rootPrefix;

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
                        var destination = "." + rootPrefix;

                        if (Current.Configuration?.Development?.QualifiedServerName != null)
                            if (context.Request.Host.Host != Current.Configuration?.Development?.QualifiedServerName)
                            {
                                var sourcePort = context.Request.Host.Port;

                                var targetProtocol = "";

                                if (Base.Host.IsDevelopment)
                                {
                                    var httpPort = Base.Host.Variables.GetValue(Host.Keys.WebHttpPort).ToType<int, object>();
                                    var httpsPort = Base.Host.Variables.GetValue(Host.Keys.WebHttpsPort).ToType<int, object>();

                                    if (sourcePort == httpPort)
                                    {
                                        sourcePort = httpsPort;
                                        targetProtocol = "https:";
                                    }
                                }

                                var destinationHost = context.Request.Host.Port.HasValue ? new HostString(Current.Configuration.Development.QualifiedServerName, sourcePort.Value) : new HostString(Current.Configuration.DevelopmentQualifiedServerName);

                                destination = $"{targetProtocol}//" + destinationHost.ToString() + rootPrefix;
                            }

                        Log.KeyValuePair(App.Current.Orchestrator.Application.ToString(), $"Redirect: {destination}", Message.EContentType.StartupSequence);

                        context.Response.Redirect(destination, false);
                        return Task.FromResult(0);
                    });
                });
            }


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            //app
            ////    //.UseHttpsRedirection()
            //    .UseMvc();

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