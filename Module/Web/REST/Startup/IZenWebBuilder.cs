using Microsoft.AspNetCore.Builder;
using Zen.Module.Web.REST.Startup;

namespace Zen.Module.Web.REST.Startup
{
    public interface IZenWebBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenWebOptions Options { get; }
    }
}