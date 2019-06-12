using System;

namespace Zen.Base.Module.Environment
{
    public interface IEnvironmentProvider
    {
        IEnvironmentDescriptor Current { get; set; }
        string CurrentCode { get; }
        ProbeItem Probe { get; set; }
        void ResetToDefault();
        void Shutdown();
        IEnvironmentDescriptor Get(string serverName);
        event EventHandler EnvironmentChanged;
    }
}