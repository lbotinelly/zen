using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.Web.Common;

namespace Zen.Web.Host
{
    /// <summary>
    ///     Provides an alternative way to bootstrap a Web app by loading a development time self-signed certificate and
    ///     using it for HTTPS requests.
    /// </summary>
    public static class Builder
    {
        /// <summary>
        ///     <para>
        ///         Initializes the http request pipeline.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">
        ///     The class containing the Configure() and ConfigureServices() methods used to define the HTTP
        ///     request pipeline.
        /// </typeparam>
        /// <param name="args">Pass-through of start-up parameters.</param>
        public static void Start<T>(string[] args) where T : class
        {
            //var isDevEnv = Base.Host.IsDevelopment;
            //var isContainer = Base.Host.IsContainer;

            Log.Add("Zen | Startup-Sequence START");

            if (!Base.Host.IsContainer)
            {
                // Pick up certificate from local Store:

                var hostBuilder = new WebHostBuilder() // Pretty standard pipeline,
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureLogging(logging =>
                    {
                        logging
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                            .ClearProviders()
                            .AddConsole()
                            .AddEventLog();
                    })
                    .UseKestrel(k =>
                    {

                        var injectors = IoC.GetClassesByInterface<IKestrelConfigurationInjector>().CreateInstances<IKestrelConfigurationInjector>().ToList();
                        foreach (var injector in injectors) injector.Handle(k);
                    })
                    .UseStartup<T>()
                    .ConfigureKestrel((context, options) =>
                    {
                        // We'll map to 0.0.0.0 in order to allow inbound connections from all adapters.
                        var localAddress = IPAddress.Parse("0.0.0.0");

                        var currentEnv = Current.ZenWebOrchestrator.Options.GetCurrentEnvironment();

                        var httpPort = Base.Host.Variables.Get(Keys.WebHttpPort, currentEnv.HttpPort);
                        var httpsPort = Base.Host.Variables.Get(Keys.WebHttpsPort, currentEnv.HttpsPort);

                        options.Listen(localAddress, httpPort);

                        var hostCertificate = GetCertificate();

                        // Only offer HTTPS if we manage to pinpoint a development time self-signed certificate, be it custom or just the default dev cert created by VS.
                        if (hostCertificate == null) return;

                        Base.Current.Log.KeyValuePair("Certificate", $"{hostCertificate.Thumbprint} | {hostCertificate.FriendlyName}");

                        options.Listen(localAddress, httpsPort,
                            listenOptions => { listenOptions.UseHttps(hostCertificate); });
                    });

                var host = hostBuilder.Build();

                host.Run();
                return;
            }

            // Vanilla stuff.
            WebHost.CreateDefaultBuilder(args).UseStartup<T>().Build().Run();
        }

        private static X509Certificate2 GetCertificate()
        {
            X509Certificate2 targetCertificate = null;

            if (Base.Host.IsDevelopment)
            {
                var targetSubject = Current.ZenWebOrchestrator.Options?.Development?.CertificateSubject ?? "localhost";

                targetCertificate = new X509Store(StoreName.Root).BySubject(targetSubject).FirstOrDefault() ??
                                    new X509Store(StoreName.My).BySubject(targetSubject).FirstOrDefault() ??
                                    new X509Store(StoreName.My).BySubject("localhost").FirstOrDefault();
            }

            if (!Base.Host.IsProduction) return targetCertificate;

            var certPath =
                $"{Base.Host.DataDirectory}{Path.DirectorySeparatorChar}certificate{Path.DirectorySeparatorChar}";

            if (!Directory.Exists(certPath))
            {
                Log.Warn($"No physical certificate storage [{certPath}]");
            }
            else
            {
                var certFile = Directory.GetFiles(certPath).FirstOrDefault();

                if (certFile == null)
                    Log.Warn($"No certificate in physical storage [{certPath}]");
                else
                    targetCertificate = new X509Certificate2(File.ReadAllBytes(certFile));
            }

            return targetCertificate;
        }
    }
}