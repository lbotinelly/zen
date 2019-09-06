using System;

namespace Zen.Storage.Provider.File.FileSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FileSystemFileStorageAttribute : ZenFileStorageAttribute
    {
        public bool ReadOnly { get; set; } = true;
        public string Descriptor { get; set; }
    }
}