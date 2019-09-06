using System;

namespace Zen.Storage.Provider.File
{
    public abstract class ZenFileStorageAttribute : Attribute
    {
        public Type Provider { get; set; }
    }
}