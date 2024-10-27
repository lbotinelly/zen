using Zen.Pebble.FlexibleData.Common.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class StringValue : IValue<string>
    {
        public StringValue() { }

        public StringValue(string source = null) => Value = source;
        public string Value { get; set; }

        public static implicit operator StringValue(string source) => string.IsNullOrEmpty(source) ? null : new StringValue(source);
        public static implicit operator string(StringValue source) => source.Value;
    }
}