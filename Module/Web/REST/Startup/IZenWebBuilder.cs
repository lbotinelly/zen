using Microsoft.AspNetCore.Builder;

namespace Zen.Module.Web.REST.Startup
{
    public interface IZenWebBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenWebOptions Options { get; }
    }
}