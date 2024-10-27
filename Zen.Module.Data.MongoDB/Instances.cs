using System;
using System.Collections.Concurrent;
using System.Linq;
using MongoDB.Driver;
using Zen.Base;
using Zen.Base.Extension;

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

                // MongoDB Atlas .NET driver on Windows OCSP advisory
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                settings.SslSettings = new SslSettings { CheckCertificateRevocation = false };
                var client = new MongoClient(settings);

                Clients[key] = client;

                string serverSuffix;

                try { serverSuffix = client.Settings.Servers.FirstOrDefault()?.Host; } catch (Exception) { serverSuffix = client.Settings.Server.Host; }

                if (serverSuffix != null) serverSuffix = "@" + serverSuffix;

                var credentialInfo = $"{client.Settings?.Credential?.Identity?.Username ?? "(anonymous)"}{serverSuffix}";

                Events.AddLog("MongoDB Client", credentialInfo);

                return client;
            }
        }
    }
}