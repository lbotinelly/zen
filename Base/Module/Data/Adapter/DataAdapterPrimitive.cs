using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Data.Adapter
{
    public abstract class DataAdapterPrimitive : IInterceptor
    {
        public IConnectionBundlePrimitive SourceBundle;

        #region Initialization

        public abstract void Setup<T>(Settings settings) where T : Data<T>;
        public abstract void Initialize<T>() where T : Data<T>;

        #endregion

        #region Common behavior

        public Func<string> GetNewKey = () => Guid.NewGuid().ToString();

        public virtual void SetConnectionString<T>() where T : Data<T>
        {
            var settings = Data<T>.Info<T>.Settings;

            var envCode = settings.EnvironmentCode;

            if (!settings.ConnectionCypherKeys?.ContainsKey(envCode) == true)
            {
                if (settings.ConnectionCypherKeys.ContainsKey("STA")) //There is a standard code available.
                    envCode = "STA";
                else
                    Current.Log.KeyValuePair(typeof(T).FullName, "No ConnectionCypherKeys for [STA] environment", Message.EContentType.Warning);
            }

            if (settings.ConnectionCypherKeys?.ContainsKey(envCode) == true) settings.ConnectionString = settings.ConnectionCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.ConnectionString = Current.Encryption.TryDecrypt(settings.ConnectionString);

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            if (string.IsNullOrEmpty(settings.ConnectionString)) Current.Log.KeyValuePair(typeof(T).FullName, "Connection Cypher Key not set", Message.EContentType.Warning);

            if (!settings.CredentialCypherKeys.ContainsKey(envCode)) return;

            //Handling credentials
            if (settings.ConnectionString.IndexOf("{credentials}", StringComparison.Ordinal) == -1) Current.Log.Warn<T>("Credentials set, but no placeholder found on connection string");

            settings.CredentialsString = settings.CredentialCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            settings.ConnectionString =
                settings.ConnectionString.Replace("{credentials}", settings.CredentialsString);
        } // ReSharper disable InconsistentNaming

        #endregion

        #region Interceptor calls

        public abstract T Get<T>(string key, Mutator mutator = null) where T : Data<T>;
        public abstract IEnumerable<T> Get<T>(IEnumerable<string> keys, Mutator mutator = null) where T : Data<T>;

        public abstract IEnumerable<T> Query<T>(string statement) where T : Data<T>;
        public abstract IEnumerable<T> Query<T>(Mutator mutator = null) where T : Data<T>;
        public abstract IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, Mutator mutator = null) where T : Data<T>;
        public abstract IEnumerable<TU> Query<T, TU>(string statement) where T : Data<T>;
        public abstract IEnumerable<TU> Query<T, TU>(Mutator mutator = null) where T : Data<T>;

        public abstract long Count<T>(Mutator mutator = null) where T : Data<T>;

        public abstract T Insert<T>(T model, Mutator mutator = null) where T : Data<T>;
        public abstract T Save<T>(T model, Mutator mutator = null) where T : Data<T>;
        public abstract T Upsert<T>(T model, Mutator mutator = null) where T : Data<T>;

        public abstract void Remove<T>(string key, Mutator mutator = null) where T : Data<T>;
        public abstract void Remove<T>(T model, Mutator mutator = null) where T : Data<T>;
        public abstract void RemoveAll<T>(Mutator mutator = null) where T : Data<T>;

        public abstract IEnumerable<T> BulkInsert<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;
        public abstract IEnumerable<T> BulkSave<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;
        public abstract IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;
        public abstract void BulkRemove<T>(IEnumerable<string> keys, Mutator mutator = null) where T : Data<T>;
        public abstract void BulkRemove<T>(IEnumerable<T> models, Mutator mutator = null) where T : Data<T>;

        #endregion
    }
}