using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Zen.Web.Common
{
    public interface IKestrelConfigurationInjector
    {
        void Handle(KestrelServerOptions kestrelServerOptions);
    }
}