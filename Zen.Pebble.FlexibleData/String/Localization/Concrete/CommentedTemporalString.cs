using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class CommentedTemporalString : ITemporalCommented<string>
    {
        public CommentedTemporalString() { }

        public CommentedTemporalString(string source, string comments = null, System.DateTime? startDate = null, System.DateTime? endDate = null)
        {
            Value = source;
            Comments = comments;
            if (startDate != null || endDate != null) Boundaries = new HistoricPrecisionBoundary(startDate, endDate);
        }

        public HistoricPrecisionBoundary Boundaries { get; set; }
        public string Comments { get; set; }

        public string Value { get; set; }

        public override string ToString() { return Value; }

        public static implicit operator CommentedTemporalString(string source) { return new CommentedTemporalString(source); }

        public static implicit operator string(CommentedTemporalString source) { return source.Value; }
    }
}