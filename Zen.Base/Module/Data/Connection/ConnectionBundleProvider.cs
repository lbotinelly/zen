using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Module.Service;

namespace Zen.Base.Module.Data.Connection {
    public class ConnectionBundleProvider : IConnectionBundleProvider
    {
        public ConnectionBundleProvider() => DefaultBundleType = IoC.GetClassesByInterface<IConnectionBundle>().FirstOrDefault();

        public Type DefaultBundleType { get; set; }
        public Dictionary<string, IConnectionBundle> Bundles { get; set; }
    }
}