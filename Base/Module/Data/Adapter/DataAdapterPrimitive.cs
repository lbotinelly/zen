using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Log;
using Zen.Base.Wrapper;

namespace Zen.Base.Module.Data.Adapter
{


    public abstract class DataAdapterPrimitive
    {
        public IInterceptor Interceptor = null;

        public virtual void SetConnectionString<T>() where T : Data<T>
        {
            var statements = Data<T>.Settings;
            var tableData = Data<T>.TableData;

            var envCode = statements.EnvironmentCode;

            if (!statements.ConnectionCypherKeys.ContainsKey(envCode))
            {
                if (statements.ConnectionCypherKeys.ContainsKey("STA")) //There is a standard code available.
                    envCode = "STA";
                else
                    throw new ArgumentException("No connection key provided for environment [{0}]".format(envCode));
            }

            statements.ConnectionString = statements.ConnectionCypherKeys[envCode];

            try
            {
                // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
                statements.ConnectionString = Current.Encryption.Decrypt(statements.ConnectionString);
            }
            catch { }

            try
            {
                // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
                statements.CredentialsString = Current.Encryption.Decrypt(statements.CredentialsString);
            }
            catch { }

            if (statements.ConnectionString == "")
                throw new ArgumentNullException(@"Connection Cypher Key not set for " + typeof(T).FullName + ". Check class definition/configuration files.");

            if (!statements.CredentialCypherKeys.ContainsKey(envCode)) return;

            //Handling credentials

            if (statements.ConnectionString.IndexOf("{credentials}", StringComparison.Ordinal) == -1)
                if (!tableData.SuppressErrors) Current.Log.Add("[{0}] {1}: Credentials set, but no placeholder found on connection string. Skipping.".format(envCode, typeof(T).FullName), Message.EContentType.Warning);

            statements.CredentialsString = statements.CredentialCypherKeys[envCode];

            try
            {
                // If it fails to decrypt, no biggie; It may be plain-text. ignore and continue.
                statements.CredentialsString = Current.Encryption.Decrypt(statements.CredentialsString);
            }
            catch { }

            statements.ConnectionString = statements.ConnectionString.Replace("{credentials}", statements.CredentialsString);
        }

      
        // ReSharper disable InconsistentNaming
        private DynamicParametersPrimitive ParameterSourceType;
        protected internal Type dynamicParameterType = null;
        protected internal bool useOutputParameterForInsertedKeyExtraction = false;

        public class ModelDefinition
        {
            public Type Type { get; set; }
            public bool Available { get; set; }
            public string Schema { get; set; }
            public string Data { get; set; }
            public string AdapterType { get; set; }
            public string EnvironmentCode { get; set; }
            public string ExceptionMessage { get; set; }

            public override string ToString()
            {
                return (Type != null ? Type.FullName : "(Undefined)") + (Available ? " " + (Schema != null ? "[Schema]" : "") + (Data != null ? "[Data]" : "") : "");
            }
        }
  
    }

    public class ParameterDefinition
    {
        public ParameterDefinition(string pIdentifier, string pPrefix)
        {
            Identifier = pIdentifier;
            Prefix = pPrefix;
        }

        public string Identifier { get; set; }
        public string Prefix { get; set; }

        public override string ToString() { return Identifier + Prefix; }
    }

    public abstract class DynamicParametersPrimitive
    {
      
   
     

   
     
    }
}