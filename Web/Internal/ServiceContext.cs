using Zen.Web.Internal.Infrastructure;

namespace Zen.Web.Internal
{
    internal class ServiceContext
    {
        public Setup.ZenServerOptions ServerOptions { get; set; }
        public IZenTrace Log { get; set; }
    }
}