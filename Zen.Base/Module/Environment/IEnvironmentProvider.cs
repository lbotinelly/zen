using Zen.Base.Common;

namespace Zen.Base.Module.Environment
{
    public interface IEnvironmentProvider : IZenProvider
    {
        IEnvironmentDescriptor Current { get; set; }
        string CurrentCode { get; }
    }

    public class EnvironmentOptions { }
}