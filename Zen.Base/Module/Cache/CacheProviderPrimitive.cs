using System;
using System.Collections.Generic;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Base.Module.Cache
{
    public abstract class CacheProviderPrimitive : ICacheProvider
    {
        public abstract void Initialize();

        public string this[string key, CacheOptions options = null]
        {
            get
            {
                if (OperationalStatus != EOperationalStatus.Operational) return null;

                try
                {
                    return GetNative(key);
                }
                catch (Exception e)
                {
                    Current.Log.Add(e);
                    return null;
                }
            }
            set
            {
                if (OperationalStatus != EOperationalStatus.Operational) return;

                try
                {
                    if (value != null) SetNative(key, value, options);
                    else if (Contains(key)) Remove(key);
                }
                catch (Exception e)
                {
                    Current.Log.Add(e);
                }
            }
        }

        public EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Undefined;
        public abstract IEnumerable<string> GetKeys(string oNamespace = null);
        public abstract bool Contains(string key);
        public abstract void Remove(string key);
        public abstract void RemoveAll();
        public abstract string GetState();
        public string ModelKey(object model)
        {
            var modelKey = model is IDataId id ? id.Id : model.GetHashCode().ToString();
            return model.GetType().FullName + "#" + modelKey;
        }

        public void Set(object model, string fullKey = null, CacheOptions options = null)
        {
            if (fullKey == null) fullKey = ModelKey(model);
            this[fullKey, options] = model.ToJson();
        }

        public T Get<T>(string fullKey)
        {
            var probe = this[fullKey];
            return probe == null ? default : probe.FromJson<T>();
        }

        public abstract void SetNative(string key, string serializedModel, CacheOptions options = null);
        public abstract string GetNative(string key);
    }
}