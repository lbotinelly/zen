using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Zen.Base.Module.Service
{
    public interface IZenAutoUseService
    {
        void Use(IApplicationBuilder app, IHostEnvironment env = null);
    }
}