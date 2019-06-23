using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Zen.Base.Startup
{
    public class ZenBuilder
    {
        public ZenBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public virtual IServiceCollection Services { get; }
    }
}