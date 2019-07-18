using Zen.Base.Common;

namespace Zen.App.Provider
{
    public interface IAppOrchestrator : IZenProvider
    {
        IZenPerson Person { get; }
        IZenApplication Application { get; }
        object Settings { get; }
        IZenPerson GetPersonByLocator(string locator);
    }
}