using System;

namespace Zen.Base.Module.Environment
{
    public class DefaultEnvironmentProvider : IEnvironmentProvider
    {
        public IEnvironmentDescriptor Current { get => DefaultEnvironmentDescriptor.Standard; set { } }

        public string CurrentCode => DefaultEnvironmentDescriptor.Standard.Code;

        ProbeItem IEnvironmentProvider.Probe { get; set; }

        public IEnvironmentDescriptor Get(string serverName) { throw new NotImplementedException(); }

        public event EventHandler EnvironmentChanged;

        public void Initialize() { Events.ShutdownSequence.Actions.Add(Shutdown); }

        public void Shutdown() { }
    }
}