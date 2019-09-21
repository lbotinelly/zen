using System;
using System.Collections.Concurrent;

namespace Zen.Base.Module.Data {
    internal static class TypeConfigurationCache
    {
        internal static readonly ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>> 
            ClassRegistration = new ConcurrentDictionary<Type, Tuple<Settings, DataConfigAttribute>>();

        public static Tuple<Settings, DataConfigAttribute> Get<T>() where T: Data<T>
        {
            var typeReference = typeof(T);

            if (!ClassRegistration.ContainsKey(typeReference)) Data<T>.New();
            return ClassRegistration[typeReference];
        }
    }
}