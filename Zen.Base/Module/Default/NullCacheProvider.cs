﻿using System.Collections.Generic;
using Zen.Base.Common;
using Zen.Base.Module.Cache;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -2)]
    public class NullCacheProvider : ICacheProvider
    {
        public void Initialize() { }

        public string Name { get; } = "No Cache";
        public string ModelKey(object model) => null;

        public Dictionary<string, ICacheConfiguration> EnvironmentConfiguration { get; set; } = null;
        public EOperationalStatus OperationalStatus { get; } = EOperationalStatus.NonOperational;
        public IEnumerable<string> GetKeys(string oNamespace = null) => null;

        public bool Contains(string key) => false;

        public void Remove(string key) { }

        public void RemoveAll() { }
        public void Set(object model, string fullKey = null) { }

        public T Get<T>(string fullKey) => default;
        public void SetNative(string key, string serializedModel) { }
        public string GetNative(string key) => null;
    }
}