using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Zen.Base.Common;

namespace Zen.Storage.Provider.Configuration
{
    [Priority(Level = -97)]
    public abstract class ZenConfigurationStorage : IZenConfigurationStorage, IZenProvider
    {
        internal List<ConfigurationStorageAttribute> attributes;
        protected ZenConfigurationStorage()
        {
            // At the end of this evaluation only one provider will be made available for the session lifetime.

            attributes = GetType().GetCustomAttributes(typeof(ConfigurationStorageAttribute), false).Select(i=> (ConfigurationStorageAttribute)i).ToList();

        }

        #region Implementation of IZenProvider


        #endregion

        #region Implementation of IZenProvider

        public void Initialize()
        {
            var targetEnvironment = Zen.Base.Current.Environment.CurrentCode;
        }

        #endregion
    }
}