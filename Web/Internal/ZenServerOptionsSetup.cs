using System;
using Microsoft.Extensions.Options;

namespace Zen.Web.Internal
{
    internal class ZenServerOptionsSetup : IConfigureOptions<Setup.ZenServerOptions>
    {
        private readonly IServiceProvider _services;
        public ZenServerOptionsSetup(IServiceProvider services) { _services = services; }
        public void Configure(Setup.ZenServerOptions options) { options.ApplicationServices = _services; }
    }
}