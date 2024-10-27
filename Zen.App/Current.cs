using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Communication;
using Zen.App.Core.Application;
using Zen.App.Model.Configuration;
using Zen.App.Orchestrator;
using Zen.App.Provider;
using Zen.Base.Module.Service;

namespace Zen.App
{
    public static class Current
    {
        private static readonly Lazy<IZenOrchestrator> OrchestratorInstance = new Lazy<IZenOrchestrator>(() => Instances.ServiceProvider.GetService<IZenOrchestrator>(), true);
        private static readonly Lazy<IEmailConfig> EmailConfigInstance = new Lazy<IEmailConfig>(() => Instances.ServiceProvider.GetService<IEmailConfig>(), true);
        private static readonly Lazy<IApplicationProvider> ApplicationProviderInstance = new Lazy<IApplicationProvider>(() => Instances.ServiceProvider.GetService<IApplicationProvider>(), true);

        private static readonly Lazy<IZenPreference> PreferenceInstance = new Lazy<IZenPreference>(() => Instances.ServiceProvider.GetService<IZenPreference>(), true);
        private static readonly Lazy<ApplicationConfiguration> ApplicationConfigurationInstance = new Lazy<ApplicationConfiguration>(() => Base.Configuration.Options.GetSection("Application").Get<ApplicationConfiguration>(), true);

        public static ApplicationConfiguration Configuration = ApplicationConfigurationInstance.Value;
        public static IApplicationProvider ApplicationProvider = ApplicationProviderInstance.Value;
        public static IZenOrchestrator Orchestrator => OrchestratorInstance.Value;
        public static IZenPreference Preference => PreferenceInstance.Value;
        public static IEmailConfig EmailConfiguration => EmailConfigInstance.Value;
    }
}