using Zen.Base.Common;

namespace Zen.Web.Common
{
    public interface IZenWebOrchestrator : IZenProvider
    {
        public Configuration.Options Options { get; set; }
    }
}