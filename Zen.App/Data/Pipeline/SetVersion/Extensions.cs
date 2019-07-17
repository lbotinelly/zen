using System;
using System.Collections.Generic;
using System.Linq;
using Zen.App.Data.Pipeline.SetVersion.Attributes;
using Zen.Base.Module;

namespace Zen.App.Data.Pipeline.SetVersion {
    public static class Extensions
    {
        private static readonly Dictionary<Type, object> Cache = new Dictionary<Type, object>();
        private static readonly object Lock = new object();

        public static ConfigurationHandler<T> Configuration<T>() where T : Data<T>
        {
            lock (Lock)
            {
                var t = typeof(T);
                if (Cache.ContainsKey(t)) return (ConfigurationHandler<T>)Cache[t];

                var e = new ConfigurationHandler<T>
                {
                    Permissions = typeof(T).GetCustomAttributes(false).OfType<SetVersionPermissionAttribute>().FirstOrDefault()
                };

                Cache.Add(t, e);

                Cache[t] = e;

                return e;
            }
        }

    }
}