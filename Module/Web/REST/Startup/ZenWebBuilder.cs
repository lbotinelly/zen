using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Module.Web.REST.Startup
{
    public class ZenWebBuilder : IZenWebBuilder
    {
        public ZenWebBuilder(IServiceCollection services) { Services = services; }

        public ZenWebBuilder(IApplicationBuilder applicationBuilder, ZenWebOptions options)
        {
            ApplicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual IServiceCollection Services { get; }
        public IApplicationBuilder ApplicationBuilder { get; }
        public ZenWebOptions Options { get; }
    }
}