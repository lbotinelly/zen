using Zen.Base.Common;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.MongoDB
{
    [Priority(Level = -2)]
    public class MongoDbDefaultBundle : ConnectionBundlePrimitive
    {
        public MongoDbDefaultBundle() => AdapterType = typeof(MongoDbAdapter<>);
    }
}