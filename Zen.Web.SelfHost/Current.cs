using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Web.SelfHost.Common;

namespace Zen.Web.SelfHost
{
    public static class Current
    {
        private static readonly Lazy<ISelfHostOrchestrator> SelfHostOrchestratorInstance = new Lazy<ISelfHostOrchestrator>(() => Base.Module.Service.Instances.ServiceProvider.GetService<ISelfHostOrchestrator>(), true);
        public static ISelfHostOrchestrator SelfHostOrchestrator = SelfHostOrchestratorInstance.Value;
    }
}