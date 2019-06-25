namespace Zen.Base.Module.Data
{
    public class Mutator
    {
        public string Container;
        public QueryTransform Transform;

        public Mutator() { }

        public Mutator(string filter) { Transform = new QueryTransform {Filter = filter}; }

        public string SetCode { get; set; }
        public string KeyPrefix => SetCode != null ? SetCode + "." : null;
    }
}