using Microsoft.Extensions.DependencyInjection;

namespace Zen.Base.Module.Service
{
    public static class Extensions
    {
        public static IServiceCollection Add(this IServiceCollection services, IZenAutoAddService autoAddService)
        {
            autoAddService.Add(services);
            return services;
        }
    }
}