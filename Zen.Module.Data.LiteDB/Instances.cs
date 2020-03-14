using System;
using System.Collections.Concurrent;
using LiteDB;
using Zen.Base;
using Zen.Base.Extension;

namespace Zen.Module.Data.LiteDB
{
    public static class Instances
    {
        public static ConcurrentDictionary<string, LiteDatabase> Databases = new ConcurrentDictionary<string, LiteDatabase>();

        private static readonly object LockObj = new object();

        public static LiteDatabase GetDatabase(string connectionString)
        {
            lock (LockObj)
            {
                var key = connectionString.Md5Hash();

                if (Databases.ContainsKey(key)) return Databases[key];

                var client = new LiteDatabase(connectionString);

                Databases[key] = client;

                Events.AddLog("LiteDatabase", connectionString);

                return client;
            }
        }

    }
}