using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Module.Data.MongoDB
{
    public static class Instances
    {
        public static ConcurrentDictionary<string, MongoClient> Clients = new ConcurrentDictionary<string, MongoClient>();

        private static readonly object LockObj = new object();

        public static MongoClient GetClient(string connectionString)
        {
            lock (LockObj)
            {
                var key = Current.Encryption.Encrypt(connectionString).Md5Hash();

                if (Clients.ContainsKey(key)) return Clients[key];

                var client = new MongoClient(connectionString);

                Clients[key] = client;

                string serverSuffix;

                try { serverSuffix = client.Settings.Servers.FirstOrDefault()?.Host; } catch (Exception) { serverSuffix = client.Settings.Server.Host; }
                if (serverSuffix != null) serverSuffix = "@" + serverSuffix;

                var credentialInfo = $"{client.Settings?.Credential?.Identity?.Username ?? "(anonymous)"}{serverSuffix}";

                Current.Log.KeyValuePair("MONGODB_CLIENT_REG", credentialInfo, Message.EContentType.StartupSequence);

                return client;
            }
        }
    }
}