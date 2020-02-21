using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Zen.Base.Extension;

namespace Zen.Base.Module.Data
{
    public class QueryTransform
    {
        public string Filter;
        public string OmniQuery;
        public string OrderBy;
        public string Statement;
        public string OutputFormat;
        public string OutputMembers;

        public Pagination Pagination = null;

        public void AddFilter(object source) { Filter = (Filter ?? "{}").MergeJson(source.ToJson()); }
    }
}