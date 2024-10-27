using System;
using System.IO;
using System.Security.Authentication;
using Newtonsoft.Json;
using Zen.Base.Extension;
using Zen.Cloud.Configuration;
using Zen.Module.Cloud.AWS.Connectors;
using Zen.Storage.Provider.Configuration;

namespace Zen.Module.Cloud.AWS.Configuration
{
    public class AwsConfigurationStorageProvider<T> : IConfigurationStorageProvider where T : class, IAwsConfigurationStorageProvider
    {
        private CloudConfigurationStorageAttribute _config;

        private S3Connector _connector;

        private object _sourceModel;

        #region Implementation of IConfigurationStorageProvider

        public void Initialize(ConfigurationStorageAttribute config)
        {
            _config = config as CloudConfigurationStorageAttribute;
            if (_config == null) return;
            _connector = new S3Connector(_config.Location);
        }

        public bool IsValid(object sourceModel)
        {
            _sourceModel = sourceModel;

            var keyExists = _connector.Exists(_config.FileName, _config.Bucket);

            if (keyExists) return true;

            if (_config.ReadOnly) return _config.DefaultIfMissing;

            _connector.PutString(_config.FileName, sourceModel.ToJson(format: Formatting.Indented), _config.Bucket);
            return true;
        }

        public object Load() { return _connector.GetString(_config.FileName, _config.Bucket).FromJson<object>() ?? _sourceModel; }

        public void Save(object sourceModel) { _connector.PutString(_config.FileName, sourceModel.ToJson(format: Formatting.Indented), _config.Bucket); }

        public string Descriptor => _config.Descriptor;


        public void SaveTemplateFile(string name, string source)
        {
            if (string.Equals(name, _config.FileName, StringComparison.CurrentCultureIgnoreCase))
                throw new AuthenticationException("Direct access to Configuration file is not allowed.");

            _connector.PutString(name, source, _config.Bucket);

        }

        public string LoadTemplateFile(string name)
        {
            if (string.Equals(name, _config.FileName, StringComparison.CurrentCultureIgnoreCase))
                throw new AuthenticationException("Direct access to Configuration file is not allowed.");

            return _connector.GetString(name, _config.Bucket);
        }

        #endregion
    }
}