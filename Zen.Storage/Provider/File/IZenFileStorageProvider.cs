namespace Zen.Storage.Provider.File
{
    public interface IZenFileStorageProvider
    {
        string Descriptor { get; }
        void Initialize(ZenFileStorageAttribute config);
        bool IsValid(object sourceModel);
        object Load();
        void Save(object sourceModel);
    }
}