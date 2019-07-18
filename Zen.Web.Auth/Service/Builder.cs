using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Web.Auth.Service {
    public class Builder : IBuilder
    {
        public Builder(IServiceCollection services) { Services = services; }

        public Builder(IApplicationBuilder applicationBuilder, Options options)
        {
            ApplicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual IServiceCollection Services { get; }
        public IApplicationBuilder ApplicationBuilder { get; }
        public Options Options { get; }
    }
}