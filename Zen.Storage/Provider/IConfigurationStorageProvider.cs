namespace Zen.Storage.Provider {
    public interface IConfigurationStorageProvider
    {
        void Load();
        void Save();
    }
}