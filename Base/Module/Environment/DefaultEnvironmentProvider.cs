using System;

namespace Zen.Base.Module.Environment
{
    public class DefaultEnvironmentProvider : IEnvironmentProvider
    {
        public IEnvironmentDescriptor Current { get => DefaultEnvironmentDescriptor.Standard; set { } }

        public string CurrentCode => DefaultEnvironmentDescriptor.Standard.Code;

        ProbeItem IEnvironmentProvider.Probe { get; set; }

        public void ResetToDefault() { }
        public void Shutdown() { }

        public IEnvironmentDescriptor Get(string serverName) { throw new NotImplementedException(); }

        public event EventHandler EnvironmentChanged;
    }
}