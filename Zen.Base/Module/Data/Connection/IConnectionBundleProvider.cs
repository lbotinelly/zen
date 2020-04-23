using System;
using System.Collections.Generic;
using Zen.Base.Common;

namespace Zen.Base.Module.Data.Connection
{
    public interface IConnectionBundleProvider: IZenProvider
    {
        Type DefaultBundleType { get; set; }
        Dictionary<string, IConnectionBundle> Bundles { get; set; }
    }
}