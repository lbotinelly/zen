using System;

namespace Zen.Storage.Provider.File
{
    public abstract class FileStorageAttribute : Attribute
    {
        public Type Provider { get; set; }
    }
}