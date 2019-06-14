using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zen.Base.Extension;

namespace Zen.Base.Module.Cache {
    public static class CacheFactory
    {
        #region Cache Management methods

        public static List<T> FetchListResultByKey<T>(Func<string, List<T>> method, string key) { return FetchListResultByKey<T>(method, key, 600); }

        public static List<T> FetchListResultByKey<T>(Func<string, List<T>> method, string key, int cacheTimeOutSeconds = 600)
        {
            var cacheid = typeof(T).CacheKey(key);

            if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
            {
                var cache = Current.Cache[cacheid].FromJson<List<T>>();
                if (cache != null) return cache;
            }

            var ret = method(key);

            if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
                Current.Cache[cacheid, null, cacheTimeOutSeconds] = ret.ToJson();

            return ret;
        }

        public static void FlushListResultByKey<T>(string key)
        {
            var cacheid = typeof(T).CacheKey(key);

            if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
            {
                Current.Cache.Remove(cacheid);
            }

        }

        public static T FetchSingleResultByKey<T>(Func<string, T> method, string key, string baseType = null, int cacheTimeOutSeconds = 600)
        {

            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational)
                return method(key);

            var cacheid = typeof(T).CacheKey(key, baseType);

            var cache = Current.Cache[cacheid].FromJson<T>();
            if (cache != null)
            {
                //Current.Log.Add("CACHE HIT " + cacheid);
                return cache;
            }

            //Current.Log.Add("CACHE MISS " + cacheid);

            var ret = method(key);

            Current.Cache[cacheid, null, cacheTimeOutSeconds] = ret.ToJson();

            return ret;
        }


        public static void FlushSingleResultByKey<T>() { FlushSingleResultByKey<T>("s"); }
        public static void FlushSingleResultByKey<T>(string key, string fullNameAlias = null)
        {

            if (Current.Cache.OperationalStatus != EOperationalStatus.Operational)
                return;

            var cacheid = typeof(T).CacheKey(key, fullNameAlias);

            Current.Cache.Remove(cacheid);
        }

        public static void FlushResultSingleton(string namespaceSpec)
        {
            if (namespaceSpec == null)
                throw new ArgumentOutOfRangeException("Invalid cache source. Specify namespaceSpec.");

            FlushResultSingleton<string>(namespaceSpec);
        }


        public static void FlushResultSingleton<T>(string namespaceSpec = null)
        {
            string cacheid;

            T cache;

            if (namespaceSpec == null)
            {
                cacheid = typeof(T).CacheKey("s");

                try
                {
                    if (typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                        if (typeof(T).GetGenericArguments()[0].IsPrimitiveType())
                            throw new ArgumentOutOfRangeException("Invalid cache source - list contains primitive type. Specify namespaceSpec.");
                        else
                            cacheid = typeof(T).GetGenericArguments()[0].CacheKey("s");
                }
                catch { }
            }
            else
                cacheid = namespaceSpec + ":s";

            Current.Cache.Remove(cacheid);
        }

        public static T FetchResultSingleton<T>(Func<T> method, object singletonLock, string namespaceSpec = null, int timeOutSeconds = 600)
        {
            string cacheid;

            T cache;

            if (namespaceSpec == null)
            {
                cacheid = typeof(T).CacheKey("s");

                try
                {
                    if (typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                        if (typeof(T).GetGenericArguments()[0].IsPrimitiveType())
                            throw new ArgumentOutOfRangeException("Invalid cache source - list contains primitive type. Specify namespaceSpec.");
                        else
                            cacheid = typeof(T).GetGenericArguments()[0].CacheKey("s");
                }
                catch { }
            }
            else
                cacheid = namespaceSpec + ":s";

            if (singletonLock == null)
                singletonLock = new object();

            var sw = new Stopwatch();
            sw.Start();

            if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
            {

                cache = Current.Cache[cacheid].FromJson<T>();
                if (cache != null)
                {
                    sw.Stop();
                    //Current.Log.Add("GET " + "edu.bucknell.webapps.Projects.Models.ReportData" + " CACHE (" + sw.ElapsedMilliseconds + " ms)");

                    return cache;
                }
            }

            lock (singletonLock)
            {
                if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
                {
                    cache = Current.Cache[cacheid].FromJson<T>();
                    if (cache != null)
                    {
                        sw.Stop();
                        //Current.Log.Add("GET " + "edu.bucknell.webapps.Projects.Models.ReportData" + " CACHE (" + sw.ElapsedMilliseconds + " ms)");
                        return cache;
                    }
                }

                var ret = method();

                if (Current.Cache.OperationalStatus == EOperationalStatus.Operational)
                    Current.Cache[cacheid, null, timeOutSeconds] = ret.ToJson();

                cache = ret;

                sw.Stop();
                //Current.Log.Add("GET " + "edu.bucknell.webapps.Projects.Models.ReportData" + " OK (" + sw.ElapsedMilliseconds + " ms)");

            }

            return cache;
        }

        #endregion
    }
}