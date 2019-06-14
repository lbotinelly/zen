using System.Data.Common;
using Zen.Base.Common;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;

namespace Zen.Module.Data.LiteDB
{
    public class LiteDbAdapter : DataAdapterPrimitive
    {
        public LiteDbAdapter()
        {
            useOutputParameterForInsertedKeyExtraction = true;
            Interceptor = new LiteDbInterceptor();
        }
    }

    [Priority(Level = -2)]
    public class LiteDbDefaultBundle : ConnectionBundlePrimitive
    {
        public LiteDbDefaultBundle()
        {
            AdapterType = typeof(LiteDbAdapter);
        }
    }
}