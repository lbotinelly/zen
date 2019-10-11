using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Zen.Base;
using Zen.Base.Extension;

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
                if (Base.Host.IsDevelopment)
                {
                    // Pick up certificate from local Store:

                    var devCertificate = GetDevCertificate();

                    var host = new WebHostBuilder() // Pretty standard pipeline,
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseKestrel()
                        .UseStartup<T>()
                        .ConfigureKestrel((context, options) =>
                        {
                            var localAddress = IPAddress.Parse("0.0.0.0"); // But we'll map to 0.0.0.0 in order to allow inbound connections from all adapters.

                            var httpPort = Current.Configuration?.Development?.HttpPort ?? 5000;
                            Base.Host.Variables[Base.Host.Keys.WebHttpPort] = httpPort;
                            // var httpPort = Base.Host.Variables.GetValue(Base.Host.Constants.WebHttpPort).ToType<int>();

                            options.Listen(
                                localAddress,
                                httpPort
                            );

                            // Only offer HTTPS if we manage to pinpoint a development time self-signed certificate, be it custom or just the default devcert created by VS.
                            if (devCertificate != null)
                            {
                                var httpsPort = Current.Configuration?.Development?.HttpsPort ?? 5001;
                                Base.Host.Variables[Base.Host.Keys.WebHttpsPort] = httpsPort;

                                options.Listen(
                                    localAddress,
                                    httpsPort,
                                    listenOptions => { listenOptions.UseHttps(devCertificate); });
                            }
                        })
                        .Build();

                    if (devCertificate != null) // Log so we know what's going on.
                        Base.Current.Log.KeyValuePair("Development Certificate", $"{devCertificate.Thumbprint} | {devCertificate.FriendlyName}");

                    host.Run();
                    return;
                }

            // Vanilla stuff.
            WebHost.CreateDefaultBuilder(args).UseStartup<T>().Build().Run();
        }

        private static X509Certificate2 GetDevCertificate()
        {
            var targetSubject = Current.Configuration?.Development?.CertificateSubject ?? "localhost";

            var targetCertificate = new X509Store(StoreName.Root).BySubject(targetSubject).FirstOrDefault() ??
                                    new X509Store(StoreName.My).BySubject(targetSubject).FirstOrDefault() ??
                                    new X509Store(StoreName.My).BySubject("localhost").FirstOrDefault();

            return targetCertificate;
        }
    }
}