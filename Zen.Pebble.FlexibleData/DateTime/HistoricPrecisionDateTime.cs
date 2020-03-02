namespace Zen.Pebble.FlexibleData.DateTime
{
    public class HistoricPrecisionDateTime
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

        public HistoricPrecisionDateTime() { }

        public HistoricPrecisionDateTime(System.DateTime? date)
        {
            if (date == null) return;


            Date = date;

            Precision = EDatePrecision.Day;

            var precisionBoundaryProbe = Date.Value;

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

        public System.DateTime? Date { get; set; }
        public EDatePrecision? Precision { get; set; }

        public static implicit operator HistoricPrecisionDateTime(System.DateTime source) => new HistoricPrecisionDateTime(source);
    }
}