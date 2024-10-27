using Zen.Base.Common;

namespace Zen.Base.Module.Environment
{
    [Priority(Level = -99)]
    public class DefaultEnvironmentProvider : IEnvironmentProvider
    {
        public EOperationalStatus OperationalStatus { get; } = EOperationalStatus.Operational;
        public string Name => "Default Environment provider";
        public virtual string GetState() => $"{OperationalStatus}";

        public IEnvironmentDescriptor Current
        {
            get => DefaultEnvironmentDescriptor.Standard;
            set { }
        }

        public string CurrentCode => DefaultEnvironmentDescriptor.Standard.Code;

        public void Initialize()
        {
            Events.ShutdownSequence.Actions.Add(Shutdown);
        }

        public void Shutdown() { }
    }
}