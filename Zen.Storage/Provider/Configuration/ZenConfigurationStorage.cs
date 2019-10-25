using System.Collections.Generic;
using System.Linq;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Extension;

namespace Zen.Storage.Provider.Configuration
{
    public abstract class ZenConfigurationStorage : IZenConfigurationStorage, IZenProvider
    {
        internal List<ZenConfigurationStorageAttribute> attributes;

        protected ZenConfigurationStorage()
        {
            // At the end of this evaluation only one provider will be made available for the session lifetime.

            attributes = GetType().GetCustomAttributes(typeof(ZenConfigurationStorageAttribute), false).Select(i => (ZenConfigurationStorageAttribute)i).ToList();
        }

        public IZenConfigurationStorageProvider Provider { get; private set; }

        #region Implementation of IZenProvider

        public void Initialize()
        {
            // Let's resolve who can get us some sweet, sweet config data.

            var viableProviders = attributes;

            if (!viableProviders.Any()) return;

            var instances = viableProviders.Select(i =>
            {
                var instance = (IZenConfigurationStorageProvider)i.Provider.CreateInstance();
                instance.Initialize(i);

                return instance;
            }).ToList();

            var validInstance = instances.FirstOrDefault(i => i.IsValid(this));

            if (validInstance == null) return;

            Provider = validInstance;

            Events.AddLog("Configuration Storage Provider", validInstance.GetType().Name + " | " + validInstance.Descriptor);

            Provider.Load().CopyPropertiesTo(this);
        }

        #endregion

        #region Implementation of IZenProvider

        #endregion
    }
}