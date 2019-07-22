using Zen.App.Orchestrator.Model;
using Zen.Base.Common;

namespace Zen.App.Provider
{
    public interface IAppOrchestrator : IZenProvider
    {
        IZenPerson Person { get; }
        IZenApplication Application { get; }
        object Settings { get; }
        IZenPerson GetPersonByLocator(string locator);
        IZenGroup GetGroupByCode(string hostGroupCode);
        IZenApplication GetApplicationByLocator(string appLocator);
        IZenApplication GetNewApplication();
        IZenApplication UpsertApplication(IZenApplication application);
    }
}