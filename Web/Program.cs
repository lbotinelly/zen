using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
           .UseKestrel(options =>
           {
               // "Configure ASP.NET Core 2.0 Kestrel for HTTPS"
               // https://stackoverflow.com/a/46336873/1845714

               // listen for HTTP
               options.Listen(IPAddress.Loopback, 80);

               // retrieve certificate from store
               using (var store = new X509Store(StoreName.My))
               {
                   store.Open(OpenFlags.ReadOnly);
                   var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
                   if (certs.Count > 0)
                   {
                       var certificate = certs[0];

                       // listen for HTTPS
                       options.Listen(IPAddress.Loopback, 443, listenOptions => { listenOptions.UseHttps(certificate); });
                   }
               }
           })
            .UseStartup<Startup>()


            ;
    }
}
