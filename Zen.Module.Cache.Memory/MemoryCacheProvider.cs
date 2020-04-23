using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Zen.Base.Common;
using Zen.Base.Module.Cache;

namespace Zen.Module.Cache.Memory
{
    [Priority(Level = -1)]
    public class MemoryCacheProvider : CacheProviderPrimitive
    {
        private readonly IMemoryCache _memoryCache;
        private CancellationTokenSource _cts;
        private MemoryCacheEntryOptions _mceo;
        public MemoryCacheProvider(IMemoryCache memoryCache) => _memoryCache = memoryCache;
        public override void Initialize()
        {
            InitializeResetToken();
            OperationalStatus = EOperationalStatus.Operational;
        }

        public override string GetState() => $"{OperationalStatus}";

        public override IEnumerable<string> GetKeys(string oNamespace) => throw new NotImplementedException();

        public override bool Contains(string key)
        {
            _memoryCache.TryGetValue(key, out var probe);
            return probe != null;
        }

        public override void Remove(string key) => _memoryCache.Remove(key);

        public override void RemoveAll()
        {
            _cts.Cancel();
            InitializeResetToken();
        }

        private void InitializeResetToken()
        {
            _cts = new CancellationTokenSource();
            _mceo = new MemoryCacheEntryOptions().AddExpirationToken(new CancellationChangeToken(_cts.Token));
        }

        public override void SetNative(string key, string serializedModel, CacheOptions options = null)
        {

            var cachingOptions = options == null ?
                _mceo :
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = options.LifeTimeSpan }
                    .AddExpirationToken(new CancellationChangeToken(_cts.Token));

            _memoryCache.Set(key, serializedModel, cachingOptions);
        }

        public override string GetNative(string key)
        {
            _memoryCache.TryGetValue(key, out string value);
            return value;
        }
    }
}