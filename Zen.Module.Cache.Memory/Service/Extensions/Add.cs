using Microsoft.Extensions.DependencyInjection;

namespace Zen.Module.Cache.Memory.Service.Extensions
{
    public static class Add
    {
        public static void AddModule(this IServiceCollection services)
        {
            services.AddMemoryCache();
        }
    }
}