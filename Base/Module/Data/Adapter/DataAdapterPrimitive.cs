using System;
using System.Collections.Generic;
using Zen.Base.Module.Data.Connection;

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

            if (!settings.ConnectionCypherKeys.ContainsKey(envCode))
            {
                if (settings.ConnectionCypherKeys.ContainsKey("STA")) //There is a standard code available.
                    envCode = "STA";
                else Current.Log.Warn<T>("No ConnectionCypherKeys for [STA] environment");
            }

            if (settings.ConnectionCypherKeys.ContainsKey(envCode)) settings.ConnectionString = settings.ConnectionCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.ConnectionString = Current.Encryption.TryDecrypt(settings.ConnectionString);

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            if (string.IsNullOrEmpty(settings.ConnectionString)) Current.Log.Warn<T>("Connection Cypher Key not set");

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
        public abstract T Get<T>(string key) where T : Data<T>;
        public abstract IEnumerable<T> Get<T>(IEnumerable<string> keys) where T : Data<T>;

        public abstract IEnumerable<T> Query<T>(string statement) where T : Data<T>;
        public abstract IEnumerable<T> Query<T>(QueryModifier modifier = null) where T : Data<T>;
        public abstract IEnumerable<TU> Query<T, TU>(string statement) where T : Data<T>;
        public abstract IEnumerable<TU> Query<T, TU>(QueryModifier modifier = null) where T : Data<T>;

        public abstract long Count<T>(string statement) where T : Data<T>;
        public abstract long Count<T>(QueryModifier modifier = null) where T : Data<T>;

        public abstract T Insert<T>(T model) where T : Data<T>;
        public abstract T Save<T>(T model) where T : Data<T>;
        public abstract T Upsert<T>(T model) where T : Data<T>;

        public abstract void Remove<T>(string key) where T : Data<T>;
        public abstract void Remove<T>(T model) where T : Data<T>;
        public abstract void RemoveAll<T>() where T : Data<T>;

        public abstract IEnumerable<T> BulkInsert<T>(IEnumerable<T> models) where T : Data<T>;
        public abstract IEnumerable<T> BulkSave<T>(IEnumerable<T> models) where T : Data<T>;
        public abstract IEnumerable<T> BulkUpsert<T>(IEnumerable<T> models) where T : Data<T>;
        public abstract void BulkRemove<T>(IEnumerable<string> keys) where T : Data<T>;
        public abstract void BulkRemove<T>(IEnumerable<T> models) where T : Data<T>;

        #endregion
    }
}