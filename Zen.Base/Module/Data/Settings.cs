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
    public enum EDataStatus
    {
        Undefined,
        Initializing,
        Operational,
        RecoverableFailure,
        CriticalFailure,
        ShuttingDown
    }

    public class PipelineQueueHandler
    {
        public List<IAfterActionPipeline> After = null;
        public List<IBeforeActionPipeline> Before = null;
    }


    public class Settings<T> where T:Data<T>
    {

        public DataAdapterPrimitive<T> Adapter;

        public IConnectionBundle Bundle;

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

        public string StorageCollectionName { get; set; }
        public List<DataEnvironmentMappingAttribute> EnvironmentMapping { get; set; }
        public FieldInfo KeyField { get; set; }
        public PropertyInfo KeyProperty { get; set; }
        public FieldInfo DisplayField { get; set; }
        public PropertyInfo DisplayProperty { get; set; }
        public bool Silent { get; set; }
        public string TypeName { get; set; }
        public string TypeQualifiedName { get; set; }
        public string TypeNamespace { get; set; }
        public Dictionary<string, MemberAttribute> Members { get; set; }
        public string FriendlyName { get; set; }
        public string StorageKeyMemberName { get; set; }
        public Lazy<T> GetInstancedModifier() => new Lazy<T>(() => (T)Activator.CreateInstance(typeof(T), null));


        public class DataState
        {
            private EDataStatus _status;
            private string _step;
            public Dictionary<DateTime, string> Events = new Dictionary<DateTime, string>();
            public DataState() { Status = EDataStatus.Undefined; }
            public EDataStatus Status
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

            public void Set(EDataStatus status, string msg) 
            {
                Status = status;
                Description = msg;

                Message.EContentType targetType;

                switch (status)
                {
                    case EDataStatus.Undefined:
                        targetType = Message.EContentType.Undefined;
                        break;
                    case EDataStatus.Initializing:
                        targetType = Message.EContentType.StartupSequence;
                        break;
                    case EDataStatus.Operational:
                        targetType = Message.EContentType.Info;
                        break;
                    case EDataStatus.RecoverableFailure:
                        targetType = Message.EContentType.Warning;
                        break;
                    case EDataStatus.CriticalFailure:
                        targetType = Message.EContentType.Critical;
                        break;
                    case EDataStatus.ShuttingDown:
                        targetType = Message.EContentType.ShutdownSequence;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }

                Current.Log.Add(typeof(T).Name + " : " + msg, targetType);
            }
        }
    }

}