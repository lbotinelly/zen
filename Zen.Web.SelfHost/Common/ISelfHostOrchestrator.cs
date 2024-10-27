using System.Threading.Tasks;
using Zen.Base.Common;

namespace Zen.Web.SelfHost.Common
{
    public interface ISelfHostOrchestrator : IZenProvider
    {
        Task Start();
    }
}