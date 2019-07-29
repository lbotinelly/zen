namespace Zen.App.Provider.Person {
    public class ZenPersonAction : IZenPersonAction {
        #region Implementation of IZenAction

        public string Action { get; set; }

        #endregion

        #region Implementation of IDataId

        public string Id { get; set; }

        #endregion

        #region Implementation of IDataLocator

        public string Locator { get; set; }

        #endregion

        #region Implementation of IDataActive

        public bool Active { get; set; }

        #endregion

        #region Implementation of IZenPersonBase

        public string Email { get; set; }
        public string Name { get; set; }

        #endregion
    }
}