using System.Collections.Generic;

namespace Zen.Base.Module.Data
{
    public class Mutator
    {
        public string Container;

        public Dictionary<string, string> PipelineMetadata = new Dictionary<string, string>();
        public QueryTransform Transform;

        public Mutator() { }

        public Mutator(string filter) { Transform = new QueryTransform {Filter = filter}; }

        public string SetCode { get => PipelineMetadata.ContainsKey(CommonMetadataKeys.Set) ? PipelineMetadata[CommonMetadataKeys.Set] : null; set => PipelineMetadata[CommonMetadataKeys.Set] = value; }
        public string KeyPrefix => SetCode!= null ? SetCode + "." : null;

        public static class CommonMetadataKeys
        {
            public static string Container = "container";
            public static string Set = "set";
        }
    }
}