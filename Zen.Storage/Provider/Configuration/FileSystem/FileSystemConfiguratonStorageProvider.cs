using System.IO;
using Newtonsoft.Json;
using Zen.Base.Extension;

namespace Zen.Storage.Provider.Configuration.FileSystem
{
    public class FileSystemConfiguratonStorageProvider<T> : IConfigurationStorageProvider
        where T : class
    {
        private FileSystemConfigurationStorageAttribute _config;
        private FileInfo _file;
        private DirectoryInfo _path;

        #region Implementation of IConfigurationStorageProvider

        public void Initialize(ConfigurationStorageAttribute config)
        {
            _config = config as FileSystemConfigurationStorageAttribute;

            if (_config == null) return;

            _path = new DirectoryInfo(_config.Location);
            _file = new FileInfo(Path.Combine(_path.FullName, _config.FileName));
        }

        public bool IsValid(object sourceModel)
        {
            if (!_path.Exists) return false;
            if (_file.Exists) return true;
            File.WriteAllText(_file.FullName, sourceModel.ToJson(format: Formatting.Indented));
            return true;
        }

        public object Load() { return File.ReadAllText(_file.FullName).FromJson<object>(); }
        public void Save(object sourceModel) { }

        #endregion
    }
}