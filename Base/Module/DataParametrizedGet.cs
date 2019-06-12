namespace Zen.Base.Module {
    public class DataParametrizedGet
    {
        public string QueryTerm { get; set; }
        public string OrderBy { get; set; }
        public long PageSize { get; set; }
        public long PageIndex { get; set; }
        public string Filter { get; set; }
        public DataParametrizedGet() { PageIndex = -1; }
    }
}