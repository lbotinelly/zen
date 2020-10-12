using System;

namespace Zen.Provider.GitHub.Storage {
    public class GitHubFileStorageConfigurationAttribute : Attribute
    {
        public string ClientName;
        public string Owner;
        public string Repository;
        public string Branch;
        public string Url;
        public string Token;
        public string BasePath;
        public bool UseSystemTempSpace;
    }
}