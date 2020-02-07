using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataConfigAttribute : Attribute
    {
        public Dictionary<string, string> CredentialCypherKeys = null;
        public bool AutoGenerateMissingSchema { get; set; } = true;
        public Type ConnectionBundleType { get; set; } = null;
        public string KeyName { get; set; }
        public string Label { get; set; }
        public bool IsReadOnly { get; set; } = false;
        public string SetName { get; set; }
        public string SetPrefix { get; set; }
        public bool UseCaching { get; set; } = true;
        public Type CredentialSetType { get; set; } = null;
        public string PersistentEnvironmentCode { get; set; }
        public bool IgnoreEnvironmentPrefix { get; set; }
        public string DisplayProperty { get; set; }
        public bool Silent { get; set; }
        public string Description { get; set; }
        public string FriendlyName { get; set; }
    }
}