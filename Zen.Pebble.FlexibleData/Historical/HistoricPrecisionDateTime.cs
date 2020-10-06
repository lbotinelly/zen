using System;
using System.Globalization;

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

        public HistoricDateTime()
        {
        }

        public HistoricDateTime(System.DateTime? date)
        {
            if (date == null) return;

            Value = date;
            Precision = EDatePrecision.Day;

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

        private string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        #region Overrides of Object

        public override string ToString()
        {
            if (Value == null) return null;

            switch (Precision)
            {
                case EDatePrecision.Millennium:
                    return AddOrdinal((int) Math.Floor((decimal) Value.Value.Year / 1000) + 1) + " millennium";

                case EDatePrecision.Century:
                    return Math.Floor((decimal) Value.Value.Year / 100).ToString(CultureInfo.InvariantCulture) + "00s";

                case EDatePrecision.Decade:
                    return Math.Floor((decimal) Value.Value.Year / 10).ToString(CultureInfo.InvariantCulture) + "0s";
                case EDatePrecision.Year:
                    return Value.Value.ToString("yyyy");

                case EDatePrecision.Season:
                    return Value.Value.ToString("MMMM yyyy");

                case EDatePrecision.Month:
                    return Value.Value.ToString("MMMM yyyy");

                case EDatePrecision.Day:
                case null:
                    return Value.Value.ToString("D");

                default: return base.ToString();
            }
        }

        #endregion

        public static implicit operator HistoricDateTime(System.DateTime source)
        {
            return new HistoricDateTime {Value = source, Precision = EDatePrecision.Day};
        }

        public static implicit operator HistoricDateTime(string source)
        {
            return string.IsNullOrEmpty(source) ? null : new HistoricDateTime(TryParse(source));
        }

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