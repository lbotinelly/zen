using System;

namespace Zen.Storage.Provider.Configuration.FileSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FileSystemConfigurationStorageAttribute : ConfigurationStorageAttribute
    {
        public string EnvironmentCode { get; set; }
        public string FileName { get; set; }
        public string Location { get; set; }
        public bool ReadOnly { get; set; }
    }
}