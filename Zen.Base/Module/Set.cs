using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Data;

namespace Zen.Base.Module
{
    public sealed class Set<T> where T : Data<T>
    {
        private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();

        public T Fetch(string key)
        {
            if (_cache.ContainsKey(key)) return _cache[key];

            var probe = Data<T>.Get(key);

            if (probe == null)
            {
                probe = typeof(T).CreateInstance<T>();
                probe.SetDataKey(key);
            }

            _cache[probe.GetDataKey()] = probe;
            return probe;
        }

        public List<T> Save()
        {
            var tempSet = _cache.Values.ToList();
            tempSet.Save();
            return tempSet;
        }
    }
}