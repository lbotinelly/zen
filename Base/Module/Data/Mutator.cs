namespace Zen.Base.Module.Data
{
    public class Mutator
    {
        public string SetCode { get; set; }
        public string Container;
        public QueryTransform Transform;
        public string KeyPrefix => SetCode != null ? SetCode + "." : null;
    }
}