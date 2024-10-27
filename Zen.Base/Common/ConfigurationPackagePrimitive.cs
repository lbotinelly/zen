using System;
using System.Collections.Generic;

namespace Zen.Base.Common
{
    public abstract class ConfigurationPackagePrimitive : IConfigurationPackage
    {
        public EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Undefined;
        public void Initialize() { }
        public virtual string GetState() => $"{OperationalStatus}";
        public Dictionary<Type, object> Provider { get; set; } = new Dictionary<Type, object>();

        public void SetPackage<T>(Type target)
        {
            Provider[typeof(T)] = target;
        }

    }
}