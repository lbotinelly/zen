using Zen.Pebble.FlexibleData.String.Localization.Concrete;

namespace Zen.Pebble.FlexibleData.String.Localization.Interface {
    public class TemporalCommented<T> : ITemporalCommented<T>
    {
        public T Value { get; set; }
        public HistoricPeriod Period { get; set; }
        public string Comments { get; set; }
    }
}