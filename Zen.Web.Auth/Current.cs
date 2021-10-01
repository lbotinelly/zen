using System;
using Zen.Web.Auth.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Web.Auth
{
    public static class Current
    {
        private static readonly Lazy<IAuthEventHandler> AuthEventHandlerInstance = new Lazy<IAuthEventHandler>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IAuthEventHandler>(), true);
        public static IAuthEventHandler AuthEventHandler = AuthEventHandlerInstance.Value;
    }
}
