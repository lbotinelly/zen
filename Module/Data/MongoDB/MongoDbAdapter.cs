using System.Data.Common;
using Zen.Base.Module.Data.Adapter;

namespace Zen.Module.Data.MongoDB {
    public class MongoDbAdapter : DataAdapterPrimitive
    {
        public MongoDbAdapter()
        {
            useOutputParameterForInsertedKeyExtraction = true;
            dynamicParameterType = typeof(MongoDbDynamicParameters);
            Interceptor = new MongoDbinterceptor(this);
        }
    }
}