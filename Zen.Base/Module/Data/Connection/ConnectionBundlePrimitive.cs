using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data.Connection
{
    public abstract class ConnectionBundlePrimitive : IConnectionBundle
    {
        public Dictionary<string, string> ConnectionCypherKeys { get; set; }
        public Type AdapterType { get; set; }
        public virtual void Validate(EValidationScope scope) { }
    }
}