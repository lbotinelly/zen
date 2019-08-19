using Zen.App.Model.Orchestration;
using Zen.App.Provider;
using Zen.Base.Common;

namespace Zen.App.Orchestrator
{
    [Priority(Level = -99)]
    public class DefaultOrchestrator : ZenOrchestratorPrimitive<Application, Group, Person, Application.Permission> { }
}