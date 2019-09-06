using System;

namespace Zen.Storage.Provider.Configuration
{
    public abstract class ZenConfigurationStorageAttribute : Attribute
    {
        public Type Provider { get; set; }
    }
}