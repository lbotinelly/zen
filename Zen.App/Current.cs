using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Provider;

namespace Zen.App
{
    public static class Current
    {
        public static IAppOrchestrator<IZenPermission> Orchestrator => AppOrchestrator.Value;
        private static readonly Lazy<IAppOrchestrator<IZenPermission>> AppOrchestrator = new Lazy<IAppOrchestrator<IZenPermission>>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IAppOrchestrator<IZenPermission>>(), true);
    }
}