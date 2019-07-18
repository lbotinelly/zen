namespace Zen.Base.Module.Data
{
    public class QueryTransform
    {
        public string Filter;
        public string OmniQuery;
        public string OrderBy;

        public Pagination Pagination = null;
        public string PartialQuery { get; set; }
    }
}