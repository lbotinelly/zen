using System.Data.Common;
using Zen.Base.Module.Data.Adapter;

namespace Zen.Module.Data.LiteDB {
    public class LiteDbAdapter : DataAdapterPrimitive
    {
        public LiteDbAdapter()
        {
            useOutputParameterForInsertedKeyExtraction = true;
            //dynamicParameterType = typeof(MongoDbDynamicParameters);
            //Interceptor = new MongoDbinterceptor(this);
        }

     
    }
}