using Microsoft.AspNetCore.Builder;
using Zen.Web.Startup;

namespace Zen.Web.Startup
{
    public interface IZenWebBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenWebOptions Options { get; }
    }
}