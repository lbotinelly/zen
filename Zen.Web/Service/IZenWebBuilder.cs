using Microsoft.AspNetCore.Builder;
using Zen.Web.Service;

namespace Zen.Web.Service
{
    public interface IZenWebBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenWebOptions Options { get; }
    }
}