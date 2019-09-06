using Newtonsoft.Json;
using Zen.Base.Extension;
using Zen.Cloud.Configuration;
using Zen.Storage.Provider.Configuration;

namespace Zen.Module.Cloud.AWS.Configuration
{
    public class AwsConfigurationStorageProvider<T> : IZenConfigurationStorageProvider where T : class, IAwsConfigurationStorageProvider
    {
        private CloudConfigurationStorageAttribute _config;

        private S3Connector _connector;

        private object _sourceModel;

        #region Implementation of IConfigurationStorageProvider

        public void Initialize(ZenConfigurationStorageAttribute config)
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

        #endregion
    }
}