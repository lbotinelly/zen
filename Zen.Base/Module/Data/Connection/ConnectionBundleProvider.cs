using System;
using System.Collections.Generic;
using System.Linq;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Base.Module.Data.Connection {

    [Priority(Level = -99)]
    public class ConnectionBundleProvider : IConnectionBundleProvider
    {
        public ConnectionBundleProvider() => DefaultBundleType = IoC.GetClassesByInterface<IConnectionBundle>().FirstOrDefault();

        public Type DefaultBundleType { get; set; }
        public Dictionary<string, IConnectionBundle> Bundles { get; set; }
        public EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Operational;
        public void Initialize() { }
        public string GetState() => $"{OperationalStatus} | Default: {DefaultBundleType.Name}";
    }
}