using System.Collections.Generic;
using System.Linq;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Extension;

namespace Zen.Storage.Provider.Configuration
{
    public abstract class ConfigurationStorage : IConfigurationStorage, IZenProvider
    {
        internal List<ConfigurationStorageAttribute> Attributes;

        protected ConfigurationStorage()
        {
            // At the end of this evaluation only one provider will be made available for the session lifetime.
            Attributes = GetType().GetCustomAttributes(typeof(ConfigurationStorageAttribute), false).Select(i => (ConfigurationStorageAttribute)i).ToList();
        }

        public IConfigurationStorageProvider Provider { get; private set; }

        #region Implementation of IZenProvider

        public EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Undefined;

        public void Initialize()
        {
            // Let's resolve who can get us some sweet, sweet config data.
            var viableProviders = Attributes;

            if (!viableProviders.Any()) return;

            var instances = viableProviders.Select(i =>
            {
                var instance = (IConfigurationStorageProvider)i.Provider.CreateInstance();
                instance.Initialize(i);

                return instance;
            }).ToList();

            var validInstance = instances.FirstOrDefault(i => i.IsValid(this));

            if (validInstance == null) return;

            Provider = validInstance;

            Events.AddLog("Configuration Storage Provider", validInstance.GetType().Name + " | " + validInstance.Descriptor);

            Provider.Load().CopyPropertiesTo(this);
        }

        public virtual string Name { get; }
        public virtual string GetState() => $"{OperationalStatus}";

        #endregion

        #region Implementation of IZenProvider

        #endregion
    }
}