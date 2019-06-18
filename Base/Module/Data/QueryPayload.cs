namespace Zen.Base.Module {
    public class QueryPayload
    {
        public string FullQuery;
        public string PartialQuery;
        public string OrderBy;
        public string Filter;
        public long? PageSize;
        public long? PageIndex;
    }
}