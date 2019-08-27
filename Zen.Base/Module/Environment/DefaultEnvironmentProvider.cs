using System;
using Zen.Base.Common;

namespace Zen.Base.Module.Environment
{
    [Priority(Level = -99)]
    public class DefaultEnvironmentProvider : IEnvironmentProvider
    {
        public IEnvironmentDescriptor Current { get => DefaultEnvironmentDescriptor.Standard; set { } }

        public string CurrentCode => DefaultEnvironmentDescriptor.Standard.Code;

        ProbeItem IEnvironmentProvider.Probe { get; set; }

        public IEnvironmentDescriptor GetByEnvironment() { throw new NotImplementedException(); }
        public IEnvironmentDescriptor GetByMachine(string serverName) { throw new NotImplementedException(); }

        public void Initialize() { Events.ShutdownSequence.Actions.Add(Shutdown); }

        public void Shutdown() { }
    }
}