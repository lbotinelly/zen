using System.Collections.Generic;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Data.Pipeline;

namespace Zen.Base.Module.Data
{
    public class Settings
    {
        public enum EStatus
        {
            Undefined,
            Initializing,
            Operational,
            RecoverableFailure,
            CriticalFailure,
            ShuttingDown
        }

        protected internal DataAdapterPrimitive Adapter;

        public List<IAfterActionPipeline> AfterActionPipeline = new List<IAfterActionPipeline>();
        public List<IBeforeActionPipeline> BeforeActionPipeline = new List<IBeforeActionPipeline>();

        public ConnectionBundlePrimitive Bundle;

        public Dictionary<string, string> ConnectionCypherKeys = new Dictionary<string, string>();
        public string ConnectionString;

        public Dictionary<string, string> CredentialCypherKeys = new Dictionary<string, string>();
        protected internal CredentialSetPrimitive CredentialSet;

        public string CredentialsString;

        public string EnvironmentCode;

        public string IdentifierProperty;
        public string LabelProperty;

        public IInterceptor Interceptor;

        public MicroEntityState State = new MicroEntityState();

        public class MicroEntityState
        {
            public MicroEntityState() { Status = EStatus.Undefined; }
            public EStatus Status { get; set; }
            protected internal string Description { get; internal set; }
            protected internal string Step { get; internal set; }
            protected internal string Stack { get; internal set; }
        }


    }
}