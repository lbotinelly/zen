using Zen.Base.Module.Cache;

namespace Zen.Module.Cache.Redis
{
    public class ConnectionConfiguration : ICacheConfiguration
    {
        public string ConnectionString { get; set; }
        public int DatabaseIndex { get; set; }
    }
}
