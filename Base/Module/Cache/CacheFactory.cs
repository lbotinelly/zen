using System;
using System.Collections.Generic;
using System.Xml.Schema;
using Zen.Base.Extension;

namespace Zen.Base.Module.Cache
{
    public static class CacheFactory
    {
        public static T FetchModel<T>(string key)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return default(T);

            var serializedModel = Current.Cache[typeof(T).CacheKey(key)];

            return serializedModel == null ? default(T) : serializedModel.FromJson<T>();
        }

        public static List<T> FetchSet<T>(Func<string, List<T>> method, string key) => FetchSet(method, key, 600);
        public static List<T> FetchSet<T>(Func<string, List<T>> method, string key, int cacheTimeOutSeconds)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return method(key);

            var cacheKey = typeof(T).CacheKey(key);

            var cacheModel = Current.Cache[cacheKey].FromJson<List<T>>();
            if (cacheModel != null) return cacheModel;

            cacheModel = method(key);

            Current.Cache[cacheKey, null, cacheTimeOutSeconds] = cacheModel.ToJson();

            return cacheModel;
        }
        public static void FlushSet<T>(string key)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return;
            Current.Cache.Remove(typeof(T).CacheKey(key));
        }
        public static T FetchModel<T>(Func<string, T> method, string key, string baseType = null, int cacheTimeOutSeconds = 600)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return method(key);

            var cacheKey = typeof(T).CacheKey(key, baseType);

            var cacheModel = Current.Cache[cacheKey].FromJson<T>();

            if (cacheModel != null) return cacheModel;

            cacheModel = method(key);

            Current.Cache[cacheKey, null, cacheTimeOutSeconds] = cacheModel.ToJson();

            return cacheModel;
        }
        public static void StoreModel<T>(string key, T model) => Current.Cache[typeof(T).CacheKey(key)] = model.ToJson();
        public static void FlushModel<T>() => FlushModel<T>("s");
        public static void FlushModel<T>(string key, string fullNameAlias = null)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return;
            Current.Cache.Remove(typeof(T).CacheKey(key, fullNameAlias));
        }
        public static void FlushSingleton(string nameSpace)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return;
            if (nameSpace == null) throw new ArgumentOutOfRangeException($"Invalid cache namespace {nameSpace}.");
            FlushSingleton<string>(nameSpace);
        }
        public static void FlushSingleton<T>(string nameSpace = null)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return;

            string cacheKey;

            if (nameSpace == null)
            {
                cacheKey = typeof(T).CacheKey("s");

                try
                {
                    if (typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                        if (typeof(T).GetGenericArguments()[0].IsPrimitiveType())
                            throw new ArgumentOutOfRangeException("Invalid cache source - list contains primitive type. Specify nameSpace.");
                        else
                            cacheKey = typeof(T).GetGenericArguments()[0].CacheKey("s");
                }
                catch { }
            }
            else { cacheKey = nameSpace + ":s"; }

            Current.Cache.Remove(cacheKey);
        }
        public static T FetchSingleton<T>(Func<T> method, object singletonLock, string nameSpace = null, int timeOutSeconds = 600)
        {
            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational) return method();

            string cacheKey;

            if (nameSpace == null)
            {
                cacheKey = typeof(T).CacheKey("s");

                if (typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                    if (typeof(T).GetGenericArguments()[0].IsPrimitiveType())
                        throw new ArgumentOutOfRangeException("Invalid cache source - list contains primitive type. Specify nameSpace.");
                    else
                        cacheKey = typeof(T).GetGenericArguments()[0].CacheKey("s");
            }
            else { cacheKey = nameSpace + ":s"; }

            if (singletonLock == null) singletonLock = new object();

            var cacheModel = Current.Cache[cacheKey].FromJson<T>();
            if (cacheModel != null) return cacheModel;

            lock (singletonLock)
            {
                cacheModel = Current.Cache[cacheKey].FromJson<T>();
                if (cacheModel != null) return cacheModel;

                var ret = method();

                Current.Cache[cacheKey, null, timeOutSeconds] = ret.ToJson();

                cacheModel = ret;
            }

            return cacheModel;
        }
    }
}