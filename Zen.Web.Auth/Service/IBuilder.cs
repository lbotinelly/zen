using Microsoft.AspNetCore.Builder;

namespace Zen.Web.Auth.Service {
    public interface IBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }
        Options Options { get; }
    }
}