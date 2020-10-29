using Zen.Base.Common;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -99)]
    public sealed class DefaultLogProvider : LogProviderBase
    {
        public DefaultLogProvider() => OperationalStatus = EOperationalStatus.Operational;
    }
}