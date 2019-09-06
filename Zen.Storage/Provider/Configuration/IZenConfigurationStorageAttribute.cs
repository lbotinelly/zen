namespace Zen.Storage.Provider.Configuration
{
    public interface IZenConfigurationStorageAttribute
    {
        bool ReadOnly { get; set; }
    }
}