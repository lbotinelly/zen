namespace Zen.Base.Module.Data
{
    public class QueryTransform
    {
        public string OmniQuery;
        public string PartialQuery { get; set; }
        public string OrderBy;
        public string Filter;

        public Pagination Pagination = null;
    }
}