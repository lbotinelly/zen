using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data.Connection
{
    public enum EValidationScope { Database }
    public interface IConnectionBundle
    {
        Type AdapterType { get; set; }
        Dictionary<string, string> ConnectionCypherKeys { get; set; }
        void Validate(EValidationScope scope);
    }
}