using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Web.App.Common;
using Zen.Web.App.Communication.Push;

namespace Zen.Web.App
{
    public static class Current
    {

        private static readonly Lazy<PushDispatcherPrimitive> PushDispatcherProvider = new Lazy<PushDispatcherPrimitive>(() => Base.Module.Service.Instances.ServiceProvider.GetService<PushDispatcherPrimitive>(), true);

        public static PushDispatcherPrimitive PushDispatcher => PushDispatcherProvider.Value;

        private static readonly Lazy<IZenWebOrchestrator> ZenWebOrchestratorInstance = new Lazy<IZenWebOrchestrator>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IZenWebOrchestrator>(), true);
        public static IZenWebOrchestrator ZenWebOrchestrator = ZenWebOrchestratorInstance.Value;

    }
}