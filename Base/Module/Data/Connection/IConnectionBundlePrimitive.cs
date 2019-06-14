using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data.Connection
{
    public interface IConnectionBundlePrimitive
    {
        Type AdapterType { get; set; }
        Dictionary<string, string> ConnectionCypherKeys { get; set; }
        void Validate(ConnectionBundlePrimitive.EValidationScope scope);
    }
}