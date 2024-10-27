using Zen.Pebble.FlexibleData.Common.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class CommentedTemporalStringValue : IValue<CommentedTemporalString>
    {
        public CommentedTemporalStringValue() { }

        public CommentedTemporalStringValue(string source, string comments = null, System.DateTime? startDate = null, System.DateTime? endDate = null)
        {
            Value = new CommentedTemporalString(source, comments, startDate, endDate);
            Comments = comments;
        }

        public string Comments { get; set; }
        public CommentedTemporalString Value { get; set; }
        public override string ToString() => Value;
        public static implicit operator CommentedTemporalStringValue(string source) => string.IsNullOrEmpty(source) ? null : new CommentedTemporalStringValue(source);
        public static implicit operator string(CommentedTemporalStringValue source) => source.Value;

    }
}