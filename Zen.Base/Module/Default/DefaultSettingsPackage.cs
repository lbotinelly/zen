using Zen.Base.Common;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -3)]
    internal class DefaultSettingsPackage : ConfigurationPackagePrimitive
    {
        public DefaultSettingsPackage()
        {
            SetPackage<ILogProvider>(typeof(DefaultLogProvider));
            SetPackage<IEnvironmentProvider>(typeof(DefaultEnvironmentProvider));
            OperationalStatus = EOperationalStatus.Operational;
        }
    }
}