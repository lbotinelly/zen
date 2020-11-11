using System;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.Historical;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class HistoricPeriod : IBoundary<HistoricDateTime>
    {
        private string _periodId;

        public HistoricPeriod() { }

        public HistoricPeriod(System.DateTime? startDate, System.DateTime? endDate = null)
        {
            if (startDate != null) Start = new HistoricDateTime(startDate);

            if (endDate != null) End = new HistoricDateTime(endDate);
        }

        public HistoricPeriod(string startDate, string endDate = null)
        {
            if (startDate != null) Start = startDate;
            if (endDate != null) End = endDate;
        }

        public string PeriodId { get => _periodId ??= Guid.NewGuid().ToString(); set => _periodId = value; }

        public HistoricDateTime Start { get; set; }
        public HistoricDateTime End { get; set; }

        public static implicit operator HistoricPeriod(System.DateTime source) { return new HistoricPeriod { Start = source, End = source }; }

        public static implicit operator HistoricPeriod(HistoricDateTime source) { return new HistoricPeriod { Start = source, End = source}; }
    }
}