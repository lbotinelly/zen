using System;
using Zen.Base.Common;

namespace Zen.Base.Module.Environment
{
    public interface IEnvironmentProvider : IZenProvider
    {
        IEnvironmentDescriptor Current { get; set; }
        string CurrentCode { get; }
        ProbeItem Probe { get; set; }
        IEnvironmentDescriptor Get(string serverName);
    }
}