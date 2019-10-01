using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zen.Base.Extension;
using Zen.Base.Module.Data.Adapter;
using Zen.Base.Module.Data.Connection;
using Zen.Base.Module.Data.Pipeline;
using Zen.Base.Module.Log;

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

        public ConnectionBundlePrimitive Bundle;

        public Dictionary<string, string> ConnectionCypherKeys = null;
        public string ConnectionString;

        public Dictionary<string, string> CredentialCypherKeys = null;
        protected internal CredentialSetPrimitive CredentialSet;

        public string CredentialsString;

        public string DisplayMemberName;

        public string EnvironmentCode;

        public string KeyMemberName;

        public PipelineQueueHandler Pipelines = null;

        public DataState State = new DataState();

        public Dictionary<string, string> Statistics = new Dictionary<string, string>();

        public string StorageName { get; set; }
        public List<EnvironmentMappingAttribute> EnvironmentMapping { get; set; }
        public FieldInfo KeyField { get; set; }
        public PropertyInfo KeyProperty { get; set; }
        public FieldInfo DisplayField { get; set; }
        public PropertyInfo DisplayProperty { get; set; }
        public bool Silent { get; set; }
        public string TypeName { get; set; }
        public string TypeQualifiedName { get; set; }
        public string TypeNamespace { get; set; }

        public Lazy<T> GetInstancedModifier<T>() where T : Data<T> { return new Lazy<T>(() => (T)Activator.CreateInstance(typeof(T), null)); }

        public class PipelineQueueHandler
        {
            public List<IAfterActionPipeline> After = null;
            public List<IBeforeActionPipeline> Before = null;
        }

        public class DataState
        {
            private EStatus _status;
            private string _step;
            public Dictionary<DateTime, string> Events = new Dictionary<DateTime, string>();
            public DataState() { Status = EStatus.Undefined; }
            public EStatus Status
            {
                get => _status;
                set
                {
                    _status = value;
                    Step = $"Status: {value}";
                }
            }

            #region Overrides of Object

            public override string ToString() { return $"{_step} | {Events.LastOrDefault().ToJson()}"; }

            #endregion

            protected internal string Description { get; internal set; }
            protected internal string Step
            {
                get => _step;
                internal set
                {
                    _step = value;
                    Events[DateTime.Now] = value;
                }
            }
            protected internal string Stack { get; internal set; }

            public void Set<T>(EStatus status, string msg) where T : Data<T>
            {
                Status = status;
                Description = msg;

                Message.EContentType targetType;

                switch (status)
                {
                    case EStatus.Undefined:
                        targetType = Message.EContentType.Undefined;
                        break;
                    case EStatus.Initializing:
                        targetType = Message.EContentType.StartupSequence;
                        break;
                    case EStatus.Operational:
                        targetType = Message.EContentType.Info;
                        break;
                    case EStatus.RecoverableFailure:
                        targetType = Message.EContentType.Warning;
                        break;
                    case EStatus.CriticalFailure:
                        targetType = Message.EContentType.Critical;
                        break;
                    case EStatus.ShuttingDown:
                        targetType = Message.EContentType.ShutdownSequence;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }

                Current.Log.Add(typeof(T).Name + " : " + msg, targetType);
            }
        }
    }
}