namespace Zen.Module.Data.Relational.Builder
{
    public class StatementMasks
    {
        public string Parameter { get; set; } = "{0}";
        public string InlineParameter { get; set; } = "{0}";
        public string Column { get; set; } = "[{0}]";

        public KeywordSet Keywords { get; set; } = new KeywordSet();
        public ValueSet Values { get; set; } = new ValueSet();

        public class KeywordSet
        {
            public string Keyset = "keyset";
            public string Equality = "=";
        }

        public class ValueSet
        {
            public object True { get; set; } = true;
            public object False { get; set; } = false;
        }
    }
}