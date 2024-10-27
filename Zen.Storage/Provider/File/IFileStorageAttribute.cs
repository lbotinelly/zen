namespace Zen.Storage.Provider.File
{
    public interface IFileStorageAttribute
    {
        bool ReadOnly { get; set; }
    }
}