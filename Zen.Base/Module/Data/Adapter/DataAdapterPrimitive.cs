using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Data.Adapter
{
    public abstract class DataAdapterPrimitive<T> : IInterceptor<T> where T: Data<T>
    {
        public string ReferenceCollectionName;
        public IConnectionBundle SourceBundle;

        #region Initialization

        public abstract void Setup(Settings<T> settings);
        public abstract void Initialize();

        #endregion

        #region Common behavior

        public Func<string> GetNewKey = () => Guid.NewGuid().ToString();

        public virtual void SetConnectionString()
        {
            var settings = Info<T>.Settings;

            var envCode = settings.EnvironmentCode;

            if (!settings.ConnectionCypherKeys?.ContainsKey(envCode) == true)
            {
                if (settings.ConnectionCypherKeys.ContainsKey("STA")) //There is a standard code available.
                    envCode = "STA";
                else Current.Log.KeyValuePair(typeof(T).FullName, "No ConnectionCypherKeys for [STA] environment", Message.EContentType.Warning);
            }

            if (settings.ConnectionCypherKeys?.ContainsKey(envCode) == true) settings.ConnectionString = settings.ConnectionCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.ConnectionString = Current.Encryption.TryDecrypt(settings.ConnectionString);

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            // if (string.IsNullOrEmpty(settings.ConnectionString)) Current.Log.KeyValuePair(typeof(T).FullName, "Connection Cypher Key not set", Message.EContentType.Warning);

            if (!settings.CredentialCypherKeys.ContainsKey(envCode)) return;

            //Handling credentials
            // if (settings.ConnectionString.IndexOf("{credentials}", StringComparison.Ordinal) == -1) Current.Log.Warn("Credentials set, but no placeholder found on connection string");

            settings.CredentialsString = settings.CredentialCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            settings.ConnectionString =
                settings.ConnectionString.Replace("{credentials}", settings.CredentialsString);
        } // ReSharper disable InconsistentNaming

        #endregion

        #region Interceptor calls

        public abstract T Get(string key, Mutator mutator = null);
        public abstract IEnumerable<T> Get(IEnumerable<string> keys, Mutator mutator = null);

        public abstract IEnumerable<T> Query(string statement);
        public abstract IEnumerable<T> Query(Mutator mutator = null);
        public abstract IEnumerable<T> Where(Expression<Func<T, bool>> predicate, Mutator mutator = null);
        public abstract IEnumerable<TU> Query<TU>(string statement);
        public abstract IEnumerable<TU> Query<TU>(Mutator mutator = null);

        public abstract long Count(Mutator mutator = null);
        public abstract bool KeyExists(string key, Mutator mutator = null);

        public abstract T Insert(T model, Mutator mutator = null);
        public abstract T Save(T model, Mutator mutator = null);
        public abstract T Upsert(T model, Mutator mutator = null);

        public abstract void Remove(string key, Mutator mutator = null);
        public abstract void Remove(T model, Mutator mutator = null);
        public abstract void RemoveAll(Mutator mutator = null);

        public abstract IEnumerable<T> BulkInsert(IEnumerable<T> models, Mutator mutator = null);
        public abstract IEnumerable<T> BulkSave(IEnumerable<T> models, Mutator mutator = null);
        public abstract IEnumerable<T> BulkUpsert(IEnumerable<T> models, Mutator mutator = null);
        public abstract void BulkRemove(IEnumerable<string> keys, Mutator mutator = null);
        public abstract void BulkRemove(IEnumerable<T> models, Mutator mutator = null);

        public abstract void DropSet(string setName);
        public abstract void CopySet(string sourceSetIdentifier, string targetSetIdentifier, bool flushDestination = false);

        #endregion
    }
}