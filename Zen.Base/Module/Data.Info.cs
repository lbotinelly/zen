using Zen.Base.Module.Cache;
using Zen.Base.Module.Data;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

#pragma warning disable 693

namespace Zen.Base.Module
{
    public abstract partial class Data<T> where T : Data<T>
    {
        public static class Info<T> where T : Data<T>
        {
            static Info() { _cacheKeyBase = typeof(T).FullName; }
            public static Settings Settings => ClassRegistration[typeof(T)].Item1;
            public static DataConfigAttribute Configuration => ClassRegistration[typeof(T)].Item2;
            public static string CacheKey(string key = "") { return _cacheKeyBase + ":" + key; }

            public static void TryFlushCachedCollection(Mutator mutator = null)
            {
                if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;

                var collectionKey = mutator?.KeyPrefix + _cacheKeyBase;
                Current.Cache.RemoveAll(collectionKey);
            }

            internal static void TryFlushCachedModel(T model, Mutator mutator = null)
            {
                if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;

                var key = mutator?.KeyPrefix + model.GetDataKey();

                CacheFactory.FlushModel<T>(key);
            }
        }
    }
}