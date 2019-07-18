using Zen.App.Orchestrator.Model;
using Zen.Base;
using Zen.Base.Module;

namespace Zen.App.Provider
{
    public abstract class AppOrchestratorPrimitive<TA, TG, TP> : IAppOrchestrator
        where TA : Data<TA>, IZenApplication
        where TG : Data<TG>, IZenGroup
        where TP : Data<TP>, IZenPerson
    {
        private IZenApplication _application;
        private object _settings;

        #region Implementation of IZenProvider

        public void Initialize()
        {
            DetectEnvironment();
            _settings = new Settings().GetSettings();

        }

        #endregion

        #region Implementation of IAppOrchestrator

        public virtual object Settings => _settings;
        public virtual IZenPerson GetPersonByLocator(string locator) { return Data<TP>.GetByLocator(locator); }
        public virtual IZenApplication GetApplicationByLocator(string locator) { return Data<TA>.GetByLocator(locator); }
        public virtual IZenPerson Person { get; set; }
        public virtual IZenApplication Application => _application;
        public void DetectEnvironment()
        {
            // Let's determine the current app.

            var currentAppLocator = Host.ApplicationAssemblyName + ".dll";
            _application = Data<TA>.GetByLocator(currentAppLocator);

            if (_application != null) return;

            var newApp = new Application
            {
                Locator = currentAppLocator,
                Code = Host.ApplicationAssemblyName,
                Name = Host.ApplicationAssemblyName
            };

            newApp.Save();

            _application = newApp;
        }

        #endregion
    }
}