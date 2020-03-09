using System;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.Historical;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class HistoricDateTimeBoundary : IBoundary<HistoricDateTime>
    {
        private string _boundaryId;

        public HistoricDateTimeBoundary() { }

        public HistoricDateTimeBoundary(System.DateTime? startDate, System.DateTime? endDate = null)
        {
            if (startDate != null) Start = new HistoricDateTime(startDate);

            if (endDate != null) End = new HistoricDateTime(endDate);
        }

        public string BoundaryId { get => _boundaryId ??= Guid.NewGuid().ToString(); set => _boundaryId = value; }

        public HistoricDateTime Start { get; set; }
        public HistoricDateTime End { get; set; }

        public static implicit operator HistoricDateTimeBoundary(System.DateTime source) { return new HistoricDateTimeBoundary {Start = source, End = source}; }
    }
}