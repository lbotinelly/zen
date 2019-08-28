using Newtonsoft.Json;
using Zen.Base.Extension;
using Zen.Cloud.Configuration;
using Zen.Storage.Provider;
using Zen.Storage.Provider.Configuration;

namespace Zen.Module.Cloud.AWS.Configuration
{
    public class AwsConfiguratonStorageProvider<T> : IConfigurationStorageProvider where T : class, IAwsConfigurationStorageProvider
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

            var keyExists = _connector.ObjectExists(_config.Bucket, _config.FileName);

            if (keyExists) return true;

            if (_config.ReadOnly) return _config.DefaultIfMissing;

            _connector.s3WriteStringToBucket(_config.Bucket, _config.FileName, sourceModel.ToJson(format: Formatting.Indented));
            return true;
        }

        public object Load() { return _connector.s3ReadStringFromBucket(_config.Bucket, _config.FileName).FromJson<object>() ?? _sourceModel; }

        public void Save(object sourceModel) { _connector.s3WriteStringToBucket(_config.Bucket, _config.FileName, sourceModel.ToJson(format: Formatting.Indented)); }

        public string Descriptor => _config.Descriptor;

        #endregion
    }
}