using Microsoft.AspNetCore.Builder;

namespace Zen.Base.Startup {
    public interface IZenBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        ZenOptions Options { get; }
    }
}