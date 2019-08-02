using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Provider;

namespace Zen.App
{
	public static class Current
    {
        public static IZenOrchestrator Orchestrator => AppOrchestrator.Value;
        private static readonly Lazy<IZenOrchestrator> AppOrchestrator = new Lazy<IZenOrchestrator>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IZenOrchestrator>(), true);
    }
}