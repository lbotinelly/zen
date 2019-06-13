using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataConfigAttribute : Attribute
    {
        public Dictionary<string, string> CredentialCypherKeys = new Dictionary<string, string>();
        public bool AutoGenerateMissingSchema { get; set; } = true;
        public Type ConnectionBundleType { get; set; } = null;
        public bool ForceMapUse { get; set; } = false;
        public string IdentifierColumnName { get; set; }
        public string Label { get; set; }
        public bool IgnoreDataOnMaintenance { get; set; } = false;
        public bool IsInsertableIdentifier { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public string TableName { get; set; }
        public string TablePrefix { get; set; }
        public bool UseCaching { get; set; } = true;
        public Type AdapterType { get; set; }
        public bool AuditAccess { get; set; } = false;
        public bool AuditChange { get; set; } = false;
        public Type CredentialSetType { get; set; } = null;
        public string PersistentEnvironmentCode { get; set; }
        public bool SuppressErrors { get; set; }
        public bool IgnoreEnvironmentPrefix { get; set; }
    }
}