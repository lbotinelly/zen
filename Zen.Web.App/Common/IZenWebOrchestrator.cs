using Zen.Base.Common;

namespace Zen.Web.App.Common
{
    public interface IZenWebOrchestrator : IZenProvider
    {
        public Configuration.IOptions Options { get; set; }
    }
}