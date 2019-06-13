using Microsoft.Extensions.DependencyInjection;

namespace Zen.Base.Startup {
    public class ZenStartupHandler
    {
        internal IServiceCollection _services;
        //internal IApplicationBuilder _app;

        public void ConfigureServices(IServiceCollection services) { _services = services; }
        //public void Configure(IApplicationBuilder app)
        //{
            
        //}

    }
}