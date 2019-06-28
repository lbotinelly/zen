using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Base.Service
{
    public class ZenBuilder : IZenBuilder
    {
        public ZenBuilder(IServiceCollection services) { Services = services; }

        public ZenBuilder(IApplicationBuilder app, IServiceCollection services) { Services = services; }

        public ZenBuilder(IApplicationBuilder app, ZenOptions options)
        {
            ApplicationBuilder = app;
            Options = options;
        }

        public virtual IServiceCollection Services { get; }

        #region Implementation of IZenBuilder

        public IApplicationBuilder ApplicationBuilder { get; }
        public ZenOptions Options { get; }

        #endregion
    }
}