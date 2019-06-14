using System;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Data.Adapter
{
    public abstract class DataAdapterPrimitive
    {
        public IInterceptor Interceptor = null;

        public virtual void SetConnectionString<T>() where T : Data<T>
        {
            var statements = Data<T>.Info<T>.Settings;
            var tableData = Data<T>.Info<T>.Configuration;

            var envCode = statements.EnvironmentCode;

            if (!statements.ConnectionCypherKeys.ContainsKey(envCode))
            {
                if (statements.ConnectionCypherKeys.ContainsKey("STA")) //There is a standard code available.
                {
                    envCode = "STA";

                }
                else Current.Log.Warn<T>($"No ConnectionCypherKeys for [STA] environment");
            }

            if (statements.ConnectionCypherKeys.ContainsKey(envCode))
                statements.ConnectionString = statements.ConnectionCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            statements.ConnectionString = Current.Encryption.TryDecrypt(statements.ConnectionString);

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            statements.CredentialsString = Current.Encryption.TryDecrypt(statements.CredentialsString);

            if (string.IsNullOrEmpty(statements.ConnectionString)) Current.Log.Warn<T>($"Connection Cypher Key not set");

            if (!statements.CredentialCypherKeys.ContainsKey(envCode)) return;

            //Handling credentials

            if (statements.ConnectionString.IndexOf("{credentials}", StringComparison.Ordinal) == -1)
                Current.Log.Warn<T>($"Credentials set, but no placeholder found on connection string");

            statements.CredentialsString = statements.CredentialCypherKeys[envCode];

            // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
            statements.CredentialsString = Current.Encryption.TryDecrypt(statements.CredentialsString);

            statements.ConnectionString = statements.ConnectionString.Replace("{credentials}", statements.CredentialsString);
        } // ReSharper disable InconsistentNaming
        protected internal Type dynamicParameterType = null;
        protected internal bool useOutputParameterForInsertedKeyExtraction = false;
    }
}