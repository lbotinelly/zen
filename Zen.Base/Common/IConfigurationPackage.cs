using System;
using System.Collections.Generic;

namespace Zen.Base.Common
{
    public interface IConfigurationPackage : IZenProvider
    {
        Dictionary<Type, object> Provider { get; set; }
    }
}