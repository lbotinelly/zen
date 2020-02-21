namespace Zen.Storage.Provider.File
{
    public interface IFileStorageProvider
    {
        string Descriptor { get; }
        void Initialize(FileStorageAttribute config);
        bool IsValid(object sourceModel);
        object Load();
        void Save(object sourceModel);
    }
}