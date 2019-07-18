using Microsoft.AspNetCore.Builder;

namespace Zen.Base.Service
{
    public interface IZenBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenOptions Options { get; }
    }
}