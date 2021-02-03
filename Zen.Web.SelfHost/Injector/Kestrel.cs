using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Zen.Web.Common;

namespace Zen.Web.SelfHost.Injector
{
    public class LettuceKestrelConfigurationInjector : IKestrelConfigurationInjector
    {
        public void Handle(KestrelServerOptions k)
        {
            var appServices = k.ApplicationServices;

            k.ConfigureHttpsDefaults(h =>
            {
                h.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                h.UseLettuceEncrypt(appServices);
            });
        }
    }
}