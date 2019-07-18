using System;
using System.Collections.Generic;
using Zen.Base.Common;
using Zen.Base.Module.Cache;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -2)]
    public class NullCacheProvider : ICacheProvider
    {
        public NullCacheProvider() { OperationalStatus = EOperationalStatus.NonOperational; }

        public string this[string key, string oSet, int cacheTimeOutSeconds] { get => null; set { } }

        public Dictionary<string, ICacheConfiguration> EnvironmentConfiguration { get; set; }

        public string ServerName { get; private set; }
        public EOperationalStatus OperationalStatus { get; set; }

        public IEnumerable<string> GetAll(string oNamespace) { return null; }

        public bool Contains(string key) { return false; }

        public void Remove(string key, string oSet = null) { }

        public void SetSingleton(object value, string fullName = null) { }

        T ICacheProvider.GetSingleton<T>(string fullName) { return default(T); }

        public void Initialize() { }

        public void RemoveAll(string oSet = null) { }

        public void Shutdown() { }

        public object GetSingleton<T>(string fullName = null) { return default(T); }
        public Action OnStartup() { return null; }
        public Action OnShutdown() { return null; }
        public Action OnMaintenance() { return null; }
    }
}