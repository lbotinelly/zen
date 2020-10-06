using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class CommentedTemporalString : TemporalCommented<string>
    {
        public CommentedTemporalString() { }

        public CommentedTemporalString(string source, string comments = null, System.DateTime? startDate = null, System.DateTime? endDate = null)
        {
            Value = source;
            Comments = comments;
            if (startDate!= null || endDate!= null) Period = new HistoricPeriod(startDate, endDate);
        }
        public override string ToString() { return Value; }

        public static implicit operator CommentedTemporalString(string source) => string.IsNullOrEmpty(source) ? null : new CommentedTemporalString(source);

        public static implicit operator string(CommentedTemporalString source) => source.Value;
    }
}