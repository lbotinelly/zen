using System;
using System.Collections.Generic;
using Zen.Base.Module.Cache;
using Zen.Base.Module.Encryption;
using Zen.Base.Module.Environment;
using Zen.Base.Module.Log;

namespace Zen.Base.Common
{
    public interface IConfigurationPackage: IZenProvider
    {
        ILogProvider Log { get; set; }
        ICacheProvider Cache { get; set; }
        IEnvironmentProvider Environment { get; set; }
        IEncryptionProvider Encryption { get; set; }
        Type GlobalConnectionBundleType { get; set; }
        string WebApiCORSDomains { get; set; }
        List<string> WebApiCORSDomainMasks { get; set; }
    }

}