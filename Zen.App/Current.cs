using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Provider;

namespace Zen.App
{
    public static class Current
    {
        public static IAppOrchestrator Orchestrator => AppOrchestrator.Value;
        private static readonly Lazy<IAppOrchestrator> AppOrchestrator = new Lazy<IAppOrchestrator>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IAppOrchestrator>(), true);
    }
}