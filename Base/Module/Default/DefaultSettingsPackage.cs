using System;
using System.Collections.Generic;
using Zen.Base.Common;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -3)]
    internal class DefaultSettingsPackage : IConfigurationPackage
    {
        public DefaultSettingsPackage()
        {
            Log = new NullLogProvider();
            Cache = new NullCacheProvider();
            Environment = new DefaultEnvironmentProvider();
            GlobalConnectionBundleType = null;
            WebApiCORSDomains = null;
        }

        public ILogProvider Log { get; set; }
        public ICacheProvider Cache { get; set; }
        public IEnvironmentProvider Environment { get; set; }
        public IEncryptionProvider Encryption { get; set; }
        public Type GlobalConnectionBundleType { get; set; }
        public string WebApiCORSDomains { get; set; }
        public List<string> WebApiCORSDomainMasks { get; set; }
    }
}