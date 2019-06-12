using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zen.Web.Internal;

namespace Zen.Web
{
    public static class Setup
    {
        public static ZenConfigurationLoader ConfigurationLoader { get; set; }

        #region Opiniated bootstrap

        internal static IWebHostBuilder ConfigureServices(this IWebHostBuilder hostBuilder, ZenServerOptions options)
        {
            if (options.SetupServer)
                hostBuilder.UseKestrel(kestrelOptions =>
                {
                    // "Configure ASP.NET Core 2.0 Kestrel for HTTPS"
                    // https://stackoverflow.com/a/46336873/1845714

                    // listen for HTTP
                    kestrelOptions.Listen(IPAddress.Loopback, 80);

                    // retrieve certificate from store
                    using (var store = new X509Store(StoreName.My))
                    {
                        store.Open(OpenFlags.ReadOnly);
                        var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
                        if (certs.Count > 0)
                        {
                            var certificate = certs[0];

                            // listen for HTTPS
                            kestrelOptions.Listen(IPAddress.Loopback, 443, listenOptions => { listenOptions.UseHttps(certificate); });
                        }
                    }
                });

            return hostBuilder;
        }

        #endregion

        public class ZenServerOptions
        {
            public bool SetupServer { get; set; } = true;
            public IServiceProvider ApplicationServices { get; set; }
        }

        #region Use

        public static IWebHostBuilder UseZen(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<ZenServerOptions>, ZenServerOptionsSetup>();
                services.AddSingleton<IServer, ZenServer>();
            });
        }

        public static IWebHostBuilder UseZen(this IWebHostBuilder hostBuilder, Action<ZenServerOptions> options = null) { return hostBuilder.UseZen().ConfigureZen(options); }

        public static IWebHostBuilder UseZen(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, ZenServerOptions> options) { return hostBuilder.UseZen().ConfigureZen(options); }

        #endregion

        #region Configure

        public static IWebHostBuilder ConfigureZen(this IWebHostBuilder hostBuilder, Action<ZenServerOptions> configureOptions)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ZenServerOptions>(options =>
                {
                    configureOptions(options);
                    hostBuilder.ConfigureServices(options);
                });

            });
        }

        public static IWebHostBuilder ConfigureZen(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, ZenServerOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Configure<ZenServerOptions>(options =>
                {
                    configureOptions(context, options);
                    hostBuilder.ConfigureServices(options);
                });
            });
        }

        #endregion
    }
}