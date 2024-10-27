using System.Collections.Generic;
using Zen.Base.Common;

namespace Zen.Base.Module.Cache
{
    public interface ICacheProvider : IZenProvider
    {
        string ModelKey(object model);
        IEnumerable<string> GetKeys(string oNamespace = null);
        bool Contains(string key);
        void Remove(string key);
        void RemoveAll();

        void Set(object model, string fullKey = null, CacheOptions options = null);
        T Get<T>(string fullKey);

        void SetNative(string key, string serializedModel, CacheOptions options = null);
        string GetNative(string key);
    }
}