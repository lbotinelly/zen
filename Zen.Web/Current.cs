using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zen.Web.Common;
using Zen.Web.Communication.Push;

namespace Zen.Web
{
    public static class Current
    {
        private static readonly Lazy<IHttpContextAccessor> ContextProvider = new Lazy<IHttpContextAccessor>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IHttpContextAccessor>(), true);

        private static readonly Lazy<PushDispatcherPrimitive> PushDispatcherProvider = new Lazy<PushDispatcherPrimitive>(() => Base.Module.Service.Instances.ServiceProvider.GetService<PushDispatcherPrimitive>(), true);

        public static HttpContext Context => ContextProvider.Value?.HttpContext;
        public static PushDispatcherPrimitive PushDispatcher => PushDispatcherProvider.Value;

        private static readonly Lazy<IZenWebOrchestrator> ZenWebOrchestratorInstance = new Lazy<IZenWebOrchestrator>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IZenWebOrchestrator>(), true);
        public static IZenWebOrchestrator ZenWebOrchestrator = ZenWebOrchestratorInstance.Value;

    }
}