using Zen.Base.Common;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.LiteDB
{
    [Priority(Level = -2)]
    public class LiteDbDefaultBundle : ConnectionBundlePrimitive
    {
        public LiteDbDefaultBundle()
        {
            AdapterType = typeof(LiteDbAdapter);
        }
    }
}