using System.Collections.Generic;
using Zen.App.Provider.Group;

namespace Zen.App.Provider.Application {
    public class ZenApplicationRepresentation : IZenApplicationRepresentation
    {
        #region Implementation of IDataId

        public string Id { get; set; }

        #endregion

        #region Implementation of IDataLocator

        public string Locator { get; set; }

        #endregion

        #region Implementation of IDataCode

        public string Code { get; set; }

        #endregion

        #region Implementation of IDataActive

        public bool Active { get; set; }

        #endregion

        #region Implementation of IZenApplicationBase

        public string Name { get; set; }

        #endregion

        #region Implementation of IZenApplicationRepresentation

        public List<ZenGroupAction> Groups { get; set; }

        #endregion
    }
}