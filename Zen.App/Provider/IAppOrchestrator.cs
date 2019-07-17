namespace Zen.App.Provider {
    public interface IAppOrchestrator {
        IZenPerson GetPersonByLocator(string locator);
        IZenPerson Person { get; set; }
        IZenApplication Application { get; set; }
    }
}