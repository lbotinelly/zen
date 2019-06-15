namespace Zen.Base.Module {
    public class QueryPayload
    {
        public string QueryTerm { get; set; }
        public string OrderBy { get; set; }
        public long PageSize { get; set; }
        public long PageIndex { get; set; }
        public string Filter { get; set; }
        public QueryPayload() { PageIndex = -1; }
    }
}