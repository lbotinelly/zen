using System;

namespace Zen.Pebble.FlexibleData.Historical
{
    public class HistoricDateTime
    {
        public enum EDatePrecision
        {
            Millennium,
            Century,
            Decade,
            Year,
            Season,
            Month,
            Day
        }

        public HistoricDateTime() { }

        public HistoricDateTime(System.DateTime? date)
        {
            if (date == null) return;

            Value = date;
            Precision = null;

            var precisionBoundaryProbe = Value.Value;

            if (precisionBoundaryProbe.Day != 1) return;
            Precision = EDatePrecision.Month;

            if (precisionBoundaryProbe.Month != 1) return;
            Precision = EDatePrecision.Year;

            if (precisionBoundaryProbe.Year % 10 != 0) return;
            Precision = EDatePrecision.Decade;

            if (precisionBoundaryProbe.Year % 100 != 0) return;
            Precision = EDatePrecision.Century;

            if (precisionBoundaryProbe.Year % 1000 != 0) return;
            Precision = EDatePrecision.Millennium;
        }

        public System.DateTime? Value { get; set; }
        public EDatePrecision? Precision { get; set; }

        public static implicit operator HistoricDateTime(System.DateTime source) => new HistoricDateTime { Value = source, Precision = EDatePrecision.Day };

        public static implicit operator HistoricDateTime(string source) => string.IsNullOrEmpty(source) ? null : new HistoricDateTime(TryParse(source));

        private static System.DateTime? TryParse(string source)
        {
            // Let's assume some common cases.

            if (source.Length == 4) // Only year.
                source += "-01-01";

            if (source.IndexOf("-00", StringComparison.InvariantCultureIgnoreCase) != -1)
                // Replace all "-00", a normal marker for undefined content, with -01 and let the internal parser deal with it.
                source = source.Replace("-00", "-01");

            return System.DateTime.Parse(source);
        }
    }
}