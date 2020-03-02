using System;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.DateTime;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class HistoricPrecisionBoundary : IBoundary<HistoricPrecisionDateTime>
    {
        private string _boundaryId;
        public string BoundaryId { get => _boundaryId ??= Guid.NewGuid().ToString(); set => _boundaryId = value; }

        public HistoricPrecisionDateTime Start { get; set; }
        public HistoricPrecisionDateTime End { get; set; }

        public HistoricPrecisionBoundary() { }

        public HistoricPrecisionBoundary(System.DateTime? startDate, System.DateTime? endDate = null)
        {
            if (startDate != null) Start = new HistoricPrecisionDateTime(startDate);

            if (endDate != null) End = new HistoricPrecisionDateTime(endDate);
        }

        public static implicit operator HistoricPrecisionBoundary(System.DateTime source) => new HistoricPrecisionBoundary { Start = source, End = source };
    }
}