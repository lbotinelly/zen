using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Web.Host;

namespace Zen.Web.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWeb(this IApplicationBuilder app, Action<IZenWebBuilder> configuration = null,
            IHostEnvironment env = null)
        {
            var optionsProvider = app.ApplicationServices.GetService<IOptions<ZenWebOptions>>();

            var options = new ZenWebOptions(optionsProvider.Value);
            var builder = new ZenWebBuilder(app, options);

            var usePrefix = Base.Host.Variables.Get(Keys.WebUsePrefix, false);

            if (!usePrefix)
            {
                // Default behavior: nothing to see here.
                app.UseDefaultFiles();
                app.UseStaticFiles();
            }
            else
            {
                // The App code will be used as prefix for all requests, so let's move the root:
                var rootPrefix = Base.Host.Variables.Get(Keys.WebRootPrefix, "");
                Events.AddLog("Web RootPrefix", rootPrefix);
                var path = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot"); // We're still using the default wwwroot folder
                Events.AddLog("Host path", path);

                if (Directory.Exists(path))
                {
                    var fileProvider = new PhysicalFileProvider(path);
                    var fOptions = new DefaultFilesOptions
                    {
                        FileProvider = fileProvider,
                        RequestPath = rootPrefix
                    };

                    app.UseDefaultFiles(
                        fOptions); // This will allow default (index.html, etc.) requests on the new mapping
                    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider, RequestPath = rootPrefix });
                }
                else
                {
                    app.UseDefaultFiles();
                    app.UseStaticFiles();
                }

                app.UseRouter(r =>
                {
                    r.MapGet("", context =>
                    {
                        var destination = "." + rootPrefix;
                        //var currentEnvOptions = Current.ZenWebOrchestrator.Options;

                        string qualifiedServerName = null; // Base.Host.Variables.Get(Keys.WebQualifiedServerName, currentEnvOptions.WebQualifiedServerName);

                        if (qualifiedServerName != null)
                            if (context.Request.Host.Host != qualifiedServerName)
                            {
                                var sourcePort = context.Request.Host.Port;
                                var targetProtocol =
                                    ""; //If we omit the protocol, the client will use the one currently set.

                                var httpPort = Base.Host.Variables.Get(Keys.WebHttpPort, Current.Options.GetCurrentEnvironment().HttpPort);
                                var httpsPort = Base.Host.Variables.Get(Keys.WebHttpsPort, Current.Options.GetCurrentEnvironment().HttpsPort);

                                if (sourcePort == httpPort)
                                {
                                    sourcePort = httpsPort;
                                    targetProtocol = "https:";
                                }

                                var destinationHost = sourcePort.HasValue
                                    ? new HostString(Current.Options.Development.QualifiedServerName, sourcePort.Value)
                                    : new HostString(qualifiedServerName);

                                destination = $"{targetProtocol}//{destinationHost}{rootPrefix}";

                                Events.AddLog("Web root", destination);
                            }

                        //Log.KeyValuePair(App.Current.Orchestrator.Application.ToString(), $"Redirect: {destination}",                            Message.EContentType.StartupSequence);

                        context.Response.Redirect(destination, false);
                        return Task.FromResult(0);
                    });
                });
            }

            //Propagates headers from incoming to outgoing requests.
            app.UseHeaderPropagation();

            app.UseRouting();

            //UseAuthentication needs to be run between UseRouting and UseEndpoints. Sooo...
            foreach (var function in Instances.BeforeUseEndpoints) function();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions()
                { 
                    AllowCachingResponses = false,
                    ResponseWriter = Diagnostics.HealthCheck.WriteResponse,
                    ResultStatusCodes = {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });

            if (Base.Host.IsDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            if (!Base.Host.IsContainer) app.UseHttpsRedirection();

            configuration?.Invoke(builder);
        }
    }
}