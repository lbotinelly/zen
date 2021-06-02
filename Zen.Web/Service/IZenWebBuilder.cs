using Microsoft.AspNetCore.Builder;

namespace Zen.Web.Service
{
    public interface IZenWebBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenWebOptions Options { get; }
    }
}