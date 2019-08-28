using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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

            if (!Base.Host.IsContainer)
                if (Base.Host.IsDevelopment)
                {
                    // Pick up certificate from local Store:

                    X509Certificate2 devCertificate = null;

                    using (var store = new X509Store(StoreName.My))
                    {
                        store.Open(OpenFlags.ReadOnly);

                        var certs = new X509Certificate2Collection();

                        if (Current.Configuration?.Development?.CertificateSubject != null)
                            certs = store.Certificates.Find(X509FindType.FindBySubjectName, Current.Configuration.Development.CertificateSubject, false);

                        if (certs.Count == 0) certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);

                        if (certs.Count > 0) devCertificate = certs[0];
                    }

                    var host = new WebHostBuilder() // Pretty standard pipeline,
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseKestrel()
                        .UseStartup<T>()
                        .ConfigureKestrel((context, options) =>
                        {
                            var localAddress = IPAddress.Parse("0.0.0.0"); // But we'll map to 0.0.0.0 in order to allow inbound connections from all adapters.

                            options.Listen(
                                localAddress,
                                Current.Configuration?.Development?.HttpPort ?? 5000
                            );

                            // Only offer HTTPS if we manage to pinpoint a development time self-signed certificate, be it custom or just the default devcert created by VS.
                            if (devCertificate != null)
                                options.Listen(
                                    localAddress,
                                    Current.Configuration?.Development?.HttpsPort ?? 5001,
                                    listenOptions => { listenOptions.UseHttps(devCertificate); });
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
    }
}