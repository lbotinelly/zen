using System;
using Zen.Base.Common;
using Zen.Base.Module.Cache;

// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

#pragma warning disable 693

namespace Zen.Base.Module.Data
{
    public static class Info<T> where T : Data<T>
    {
        private static T _Instance;
        public static Settings<T> Settings => TypeConfigurationCache<T>.Get().Item1;
        public static DataConfigAttribute Configuration => TypeConfigurationCache<T>.Get().Item2;

        public static T Instance
        {
            get
            {
                if (_Instance != null) return _Instance;

                _Instance = (T) Activator.CreateInstance(typeof(T), null);
                return _Instance;
            }
        }

        public static string CacheKey(string key = "") => Data<T>._cacheKeyBase + ":" + key;

        public static void TryFlushCachedCollection(Mutator mutator = null)
        {
            if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;
            Current.Cache.RemoveAll();
        }

        internal static void TryFlushCachedModel(T model, Mutator mutator = null)
        {
            if (!(Configuration?.UseCaching == true && Current.Cache.OperationalStatus == EOperationalStatus.Operational)) return;

            var key = mutator?.KeyPrefix + model.GetDataKey();

            CacheFactory.FlushModel<T>(key);
        }
    }
}