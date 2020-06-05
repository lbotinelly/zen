using System;
using System.Collections.Generic;

namespace Zen.Pebble.Database.Common
{
    public class StatementMasks
    {
        public string Parameter { get; set; } = "{0}";
        public string InlineParameter { get; set; } = "{0}";
        public MarkerSet Markers { get; set; } = new MarkerSet();
        public ValueSet Values { get; set; } = new ValueSet();
        public string EnumType { get; set; }
        public int MaximumTextSize { get; set; }
        public string TextOverflowType { get; set; }
        public char FieldDelimiter { get; set; }
        public int DefaultTextSize { get; set; }
        public class MarkerSet
        {
            public string KeySet = "keyset";
            public string Equality = "=";
            public char Spacer { get; set; } = '_';
        }

        public class ValueSet
        {
            public object True { get; set; } = true;
            public object False { get; set; } = false;
        }

        public Dictionary<Type, TypeMapEntry> TypeMap = new Dictionary<Type, TypeMapEntry>();

        public class TypeMapEntry
        {
            public string Name;
            public string DefaultValue;

            public TypeMapEntry(string name, string defaultValue = null)
            {
                Name = name;
                DefaultValue = defaultValue;
            }
        }
    }
}