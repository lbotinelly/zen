using Microsoft.Extensions.DependencyInjection;

namespace Zen.Base.Module.Service
{
    public interface IZenAutoAddService
    {
        void Add(IServiceCollection services);
    }
}