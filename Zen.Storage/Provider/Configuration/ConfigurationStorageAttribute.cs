using System;

namespace Zen.Storage.Provider.Configuration
{
    public abstract class ConfigurationStorageAttribute : Attribute
    {
        public Type Provider { get; set; }
    }
}