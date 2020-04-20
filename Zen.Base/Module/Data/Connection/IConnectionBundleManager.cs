using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Module.Service;

namespace Zen.Base.Module.Data.Connection
{
    public interface IConnectionBundleProvider
    {
        Type DefaultBundleType { get; set; }
        Dictionary<string, IConnectionBundle> Bundles { get; set; }
    }

    public class ConnectionBundleProvider : IConnectionBundleProvider
    {
        public ConnectionBundleProvider() => DefaultBundleType = IoC.GetClassesByInterface<IConnectionBundle>().FirstOrDefault();

        public Type DefaultBundleType { get; set; }
        public Dictionary<string, IConnectionBundle> Bundles { get; set; }
    }

    public class ConnectionBundleOptions
    {
        public Dictionary<string, IConnectionBundle> Bundles { get; set; }
    }
}