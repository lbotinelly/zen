using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data.Connection
{
    public interface IConnectionBundleProvider
    {
        Type DefaultBundleType { get; set; }
        Dictionary<string, IConnectionBundle> Bundles { get; set; }
    }
}