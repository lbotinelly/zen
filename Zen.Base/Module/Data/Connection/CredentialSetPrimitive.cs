using System;
using System.Collections.Generic;
using Zen.Base.Common;

namespace Zen.Base.Module.Data.Connection
{
    [Priority(Level = -99)]
    public class CredentialSetPrimitive
    {
        public Type AssociatedBundleType { get; set; }
        public Dictionary<string, string> CredentialCypherKeys { get; set; }
    }
}