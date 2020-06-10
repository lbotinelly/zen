namespace Zen.Pebble.Database.Common
{
    public class StatementMasks
    {
        public string ParameterPrefix { get; set; } = "@p";
        public string InlineParameter { get; set; } = "{0}";
        public MarkerSet Markers { get; set; } = new MarkerSet();
        public ValueSet BooleanValues { get; set; } = new ValueSet();
        public string EnumType { get; set; }
        public int MaximumTextSize { get; set; }
        public string TextOverflowType { get; set; }
        public char LeftFieldDelimiter { get; set; }
        public char RightFieldDelimiter { get; set; }
        public int DefaultTextSize { get; set; }
        public string SetParameterName { get; set; } = "parmset";

        public class MarkerSet
        {
            public string Equality = "=";
            public string Assign = "=";
            public string KeySet = "keyset";
            public char Spacer { get; set; } = '_';
        }

        public class ValueSet
        {
            public object True { get; set; } = true;
            public object False { get; set; } = false;
        }
    }
}