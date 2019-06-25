namespace Zen.Base.Module.Data.Connection
{
    public interface IStorageContainerResolver
    {
        string GetStorageContainerName(string environmentCode = null);
    }
}