namespace Zen.Module.Cloud.AWS.Configuration {
    public interface IAwsConfigurationStorageProvider
    {
        string ResolveTargetContainer();
    }
}