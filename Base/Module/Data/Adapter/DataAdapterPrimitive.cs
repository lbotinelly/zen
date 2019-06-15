using System;

namespace Zen.Base.Module.Data.Adapter
{
    public abstract class DataAdapterPrimitive
    {
        public IInterceptor Interceptor = null;

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

            if (settings.ConnectionCypherKeys.ContainsKey(envCode))
                settings.ConnectionString = settings.ConnectionCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.ConnectionString = Current.Encryption.TryDecrypt(settings.ConnectionString);

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            if (string.IsNullOrEmpty(settings.ConnectionString)) Current.Log.Warn<T>("Connection Cypher Key not set");

            if (!settings.CredentialCypherKeys.ContainsKey(envCode)) return;

            //Handling credentials
            if (settings.ConnectionString.IndexOf("{credentials}", StringComparison.Ordinal) == -1)
                Current.Log.Warn<T>("Credentials set, but no placeholder found on connection string");

            settings.CredentialsString = settings.CredentialCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            settings.CredentialsString = Current.Encryption.TryDecrypt(settings.CredentialsString);

            settings.ConnectionString =
                settings.ConnectionString.Replace("{credentials}", settings.CredentialsString);
        } // ReSharper disable InconsistentNaming
    }
}