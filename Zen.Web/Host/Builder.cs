﻿using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

            var useIisIntegration = Current.Options.UseIisIntegration;

            if (useIisIntegration)
            {
                var localHostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

                localHostBuilder
                    .ConfigureLogging(logging =>
                     {
                         logging
                             .AddFilter("Microsoft", LogLevel.Warning)
                             .AddFilter("System", LogLevel.Warning)
                             .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                             .ClearProviders()
                             .AddConsole();
                     });

                localHostBuilder.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<T>(); });
                localHostBuilder.Build().Run();
                return;
            }
            else
            {



                IWebHostBuilder hostBuilder = WebHost.CreateDefaultBuilder() // Pretty standard pipeline,
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureLogging(logging =>
                    {
                        logging
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                            .ClearProviders()
                            .AddConsole();
                    });



                //if (!Base.Host.IsContainer)
                //{
                hostBuilder
                    .UseKestrel(k =>
                    {

                        var injectors = IoC.GetClassesByInterface<IKestrelConfigurationInjector>().CreateInstances<IKestrelConfigurationInjector>().ToList();
                        foreach (var injector in injectors) injector.Handle(k);
                    })
                    .ConfigureKestrel((context, options) =>
                    {
                        // We'll map to 0.0.0.0 in order to allow inbound connections from all adapters.
                        var localAddress = IPAddress.Any;

                        var currentEnv = Current.Options;

                        var httpPort = currentEnv.HttpPort;
                        var httpsPort = currentEnv.HttpsPort;

                        options.Listen(localAddress, httpPort);

                        var hostCertificate = GetCertificate();

                        // Only offer HTTPS if we manage to pinpoint a development time self-signed certificate, be it custom or just the default dev cert created by VS.
                        if (hostCertificate == null)
                        {
                            Base.Current.Log.KeyValuePair("Certificate", "No certificate found. HTTPS disabled.");
                            return;
                        }

                        Base.Current.Log.KeyValuePair("Certificate", $"{hostCertificate.Thumbprint} | {hostCertificate.FriendlyName}");

                        options.Listen(localAddress, httpsPort, listenOptions =>
                        {
                            listenOptions.UseHttps(adapterOptions =>
                            {
                                adapterOptions.ServerCertificate = hostCertificate;

                            });
                        });
                    });
                //}

                hostBuilder.UseStartup<T>();

                var host = hostBuilder.Build();

                host.Run();
            }
            return;
        }

        private static X509Certificate2 GetCertificate()
        {
            X509Certificate2 targetCertificate = null;

            var targetSubject = Current.Options?.CertificateSubject;

            if (targetSubject != null)
                targetCertificate = new X509Store(StoreName.My).BySubject(targetSubject).FirstOrDefault()
                    //new X509Store(StoreName.Root).BySubject(targetSubject).FirstOrDefault() 
                    //new X509Store(StoreName.CertificateAuthority).BySubject(targetSubject).FirstOrDefault() ??
                    //new X509Store(StoreName.TrustedPeople).BySubject(targetSubject).FirstOrDefault() ??
                    //new X509Store(StoreName.TrustedPublisher).BySubject(targetSubject).FirstOrDefault()
                    ;

            //if (Base.Host.IsDevelopment)
            //    if (targetCertificate == null)
            //        targetCertificate = new X509Store(StoreName.My).BySubject("localhost").FirstOrDefault();

            if (targetCertificate != null)
            {
                Log.KeyValuePair("X509Store certificate", targetCertificate.ToString(), Base.Module.Log.Message.EContentType.Info);
                return targetCertificate;
            }

            string certFile = Current.Options?.CertificateFile;
            string certPass = Current.Options?.CertificatePassword;

            if (certFile == null)
            {

                var certPath = $"{Base.Host.DataDirectory}{Path.DirectorySeparatorChar}certificate{Path.DirectorySeparatorChar}";

                if (!Directory.Exists(certPath))
                {
                    Log.Warn($"No physical certificate storage [{certPath}]");
                }
                else
                {
                    certFile = Directory.GetFiles(certPath).FirstOrDefault(i=> i.EndsWith(".pfx"));
                    if (certFile == null)
                        Log.Warn($"No certificate in physical storage [{certPath}]");
                    else
                    {
                        if (certPass==null)   
                        certPass = File.ReadAllText(certFile.Replace(".pfx", ".pwd"));
                    }
                }

            }


            if (certFile != null)
            {

                if (!File.Exists(certFile))
                {
                    Log.KeyValuePair("Physical certificate", "Not Found", Base.Module.Log.Message.EContentType.Warning);
                    Log.KeyValuePair("Certificate path", certFile, Base.Module.Log.Message.EContentType.Warning);
                }
                else
                {
                    Log.KeyValuePair("Physical certificate", certFile, Base.Module.Log.Message.EContentType.Info);

                    if (certPass != null)
                        targetCertificate = new X509Certificate2(File.ReadAllBytes(certFile), certPass);
                }
            }


            return targetCertificate;
        }
    }
}