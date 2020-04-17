using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Zen.Base.Common;
using Zen.Base.Module.Cache;

namespace Zen.Module.Cache.Memory
{
    [Priority(Level = -1)]
    public class MemoryCacheProvider : PrimitiveCacheProvider
    {
        private readonly IMemoryCache _memoryCache;
        public MemoryCacheProvider(IMemoryCache memoryCache) => _memoryCache = memoryCache;

        public override void Initialize()
        {
            OperationalStatus = EOperationalStatus.Operational;
        }

        public override IEnumerable<string> GetKeys(string oNamespace) => throw new NotImplementedException();

        public override bool Contains(string key)
        {
            _memoryCache.TryGetValue(key, out var probe);
            return probe != null;
        }

        public override void Remove(string key) => _memoryCache.Remove(key);

        public override void RemoveAll()
        {
            // IMemoryCache does not support iteration.
        }

        public override string Name { get; } = "In-memory cache";

        public override void SetNative(string key, string serializedModel) => _memoryCache.Set(key, serializedModel);

        public override string GetNative(string key)
        {
            _memoryCache.TryGetValue(key, out string value);
            return value;
        }
    }
}