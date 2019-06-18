using System;
using System.Collections.Concurrent;
using System.Linq;
using MongoDB.Driver;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Module.Data.MongoDB {
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

                string server;

                try { server = client.Settings.Servers.FirstOrDefault()?.Host; } catch (Exception) { server = client.Settings.Server.Host; }
                if (server != null) server = " @ " + server;

                Current.Log.Add($"MONGODB_CLIENT_REGISTER {client.Settings?.Credential?.Identity?.Username ?? "(anonymous)"}{server}", Message.EContentType.StartupSequence);

                return client;
            }
        }
    }
}