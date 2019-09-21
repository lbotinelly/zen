using Zen.Base.Module.Cache;
using Zen.Base.Module.Data;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

#pragma warning disable 693

namespace Zen.Base.Module.Data
{
    public static class Info<T> where T : Data<T>
    {
        public static Settings Settings => TypeConfigurationCache.Get<T>().Item1;
        public static DataConfigAttribute Configuration => TypeConfigurationCache.Get<T>().Item2;
        public static string CacheKey(string key = "") { return Data<T>._cacheKeyBase + ":" + key; }

        public static void TryFlushCachedCollection(Mutator mutator = null)
        {
            if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;

            var collectionKey = mutator?.KeyPrefix + Data<T>._cacheKeyBase;
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