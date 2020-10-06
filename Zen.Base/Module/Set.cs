using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Data;

namespace Zen.Base.Module
{
    public sealed class Set<T> where T : Data<T>
    {
        internal Dictionary<string, T> Cache = new Dictionary<string, T>();

        public T Fetch(string key, bool ignoreCache = false)
        {
            if (!ignoreCache)
                if (Cache.ContainsKey(key))
                    return Cache[key];

            var probe = Data<T>.Get(key);

            if (probe == null)
            {
                probe = typeof(T).CreateInstance<T>();
                probe.SetDataKey(key);
            }

            Cache[probe.GetDataKey()] = probe;
            return probe;
        }

        public List<T> Save()
        {
            var tempSet = Cache.Values.ToList();
            tempSet.Save();
            return tempSet;
        }
    }

    public static class Extensions
    {
        public static Set<T> ToSet<T>(this IEnumerable<T> source) where T : Data<T> { return new Set<T> {Cache = source.ToDictionary(i => i.GetDataKey(), i => i)}; }
    }
}