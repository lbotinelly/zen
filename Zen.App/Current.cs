using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Model.Configuration;
using Zen.App.Provider;
using Zen.Base.Module.Service;

namespace Zen.App
{
    public static class Current
    {
        private static readonly Lazy<IZenOrchestrator> AppOrchestrator = new Lazy<IZenOrchestrator>(() => Instances.ServiceProvider.GetService<IZenOrchestrator>(), true);
        private static readonly Lazy<ApplicationConfiguration> LazyConfiguration = new Lazy<ApplicationConfiguration>(() => Base.Configuration.Options.GetSection("Application").Get<ApplicationConfiguration>(), true);

        public static ApplicationConfiguration Configuration = LazyConfiguration.Value;
        public static IZenOrchestrator Orchestrator => AppOrchestrator.Value;
    }
}