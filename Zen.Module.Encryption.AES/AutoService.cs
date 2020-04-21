using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Base.Module.Service;

namespace Zen.Module.Encryption.AES
{
    public class AutoService : IZenAutoAddService
    {
        public void Add(IServiceCollection services)
        {
            services.Configure<Configuration.Options>(options => options.GetSettings<Configuration.IOptions, Configuration.Options>("Encryption:AES"));
        }
    }
}