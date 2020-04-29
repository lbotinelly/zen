using System;
using System.Collections.Concurrent;

namespace Zen.Base.Module.Data
{
    internal static class TypeConfigurationCache<T> where T : Data<T>
    {
        internal static readonly ConcurrentDictionary<Type, Tuple<Settings<T>, DataConfigAttribute>> ClassRegistration = new ConcurrentDictionary<Type, Tuple<Settings<T>, DataConfigAttribute>>();

        public static Tuple<Settings<T>, DataConfigAttribute> Get()
        {
            var typeReference = typeof(T);

            if (!ClassRegistration.ContainsKey(typeReference)) Data<T>.New();
            return ClassRegistration[typeReference];
        }
    }
}