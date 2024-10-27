using System;

namespace Zen.Storage.Provider.Configuration.FileSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FileSystemConfigurationStorageAttribute : ConfigurationStorageAttribute
    {
        public string FileName { get; set; }
        public string Location { get; set; }
        public bool ReadOnly { get; set; } = true;
        public string Descriptor { get; set; }
        public bool DefaultIfMissing { get; set; } = true;
    }
}