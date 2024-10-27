namespace Zen.Storage.Provider.Configuration
{
    public interface IConfigurationStorageAttribute
    {
        bool ReadOnly { get; set; }
    }
}