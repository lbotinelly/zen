using System;
using System.Collections.Generic;

namespace Zen.Base.Module
{
    public sealed class MultiSet
    {
        private readonly Dictionary<Type, object> _setMap = new Dictionary<Type, object>();

        public T Fetch<T>(string identifier) where T : Data<T> => GetSet<T>().Fetch(identifier);

        public void Save()
        {
            foreach (var o in _setMap) ((ISetSave) o.Value).Commit();
        }

        private Set<T> GetSet<T>() where T : Data<T>
        {
            var targetType = typeof(T);

            if (_setMap.ContainsKey(targetType)) return (Set<T>) _setMap[targetType];
            _setMap[targetType] = new Set<T>();
            return (Set<T>) _setMap[targetType];
        }
    }
}