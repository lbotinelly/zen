using Zen.Base.Module;

namespace Zen.App.Provider
{
    public abstract class AppOrchestratorPrimitive<TA, TG, TP> : IAppOrchestrator
        where TA : Data<TA>, IZenApplication
        where TG : Data<TG>, IZenGroup
        where TP : Data<TP>, IZenPerson
    {
        #region Implementation of IAppOrchestrator

        public IZenPerson GetPersonByLocator(string locator) => Data<TP>.GetByLocator(locator);
        public IZenApplication GetApplicationByLocator(string locator) => Data<TA>.GetByLocator(locator);
        public IZenPerson Person { get; set; }
        public IZenApplication Application { get; set; }

        #endregion
    }
}