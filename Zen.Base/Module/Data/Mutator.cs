using System;
using System.Collections.Generic;

namespace Zen.Base.Module.Data
{
    public class Mutator
    {
        public static class CommonMetadataKeys
        {
            public static string Container = "Container";
            public static string Set = "Set";
        }

        public string Container;
        public QueryTransform Transform;

        public Mutator() { }

        public Dictionary<string, string> PipelineMetadata = new Dictionary<string, string>();

        public Mutator(string filter) { Transform = new QueryTransform {Filter = filter}; }

        public string SetCode
        {
            get => PipelineMetadata.ContainsKey(CommonMetadataKeys.Set) ? PipelineMetadata[CommonMetadataKeys.Set] : null;
            set => PipelineMetadata[CommonMetadataKeys.Set] = value;
        }
        public string KeyPrefix => SetCode != null ? SetCode + "." : null;
    }
}