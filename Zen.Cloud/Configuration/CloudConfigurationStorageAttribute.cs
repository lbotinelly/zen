using System;
using Zen.Storage.Provider.Configuration;

namespace Zen.Cloud.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CloudConfigurationStorageAttribute : ConfigurationStorageAttribute
    {
        public bool ReadOnly { get; set; } = true;
        public string FileName { get; set; } = "zenFrameworkSettings.json";
        public string Descriptor { get; set; }
        public bool DefaultIfMissing { get; set; } = true;
        public string Bucket { get; set; }
        public string Location { get; set; }
        public string HostingEnvironment { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
    }
}