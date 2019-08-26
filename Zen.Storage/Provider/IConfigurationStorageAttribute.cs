namespace Zen.Storage.Provider
{
    public interface IConfigurationStorageAttribute
    {
        bool ReadOnly { get; set; }
    }
}