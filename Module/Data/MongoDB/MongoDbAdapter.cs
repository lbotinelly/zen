using Zen.Base.Module.Data.Adapter;

namespace Zen.Module.Data.MongoDB
{
    public class MongoDbAdapter : DataAdapterPrimitive
    {
        public MongoDbAdapter()
        {
            Interceptor = new MongoDbinterceptor(this);
        }
    }
}