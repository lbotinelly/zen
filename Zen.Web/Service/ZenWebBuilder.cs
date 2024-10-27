using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Web.Service
{
    public class ZenWebBuilder : IZenWebBuilder
    {
        public ZenWebBuilder(IServiceCollection services) { Services = services; }

        public ZenWebBuilder(IApplicationBuilder applicationBuilder)
        {
            ApplicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
        }

        public virtual IServiceCollection Services { get; }
        public IApplicationBuilder ApplicationBuilder { get; }
    }
}