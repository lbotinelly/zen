using Zen.Pebble.FlexibleData.String.Localization.Concrete;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.Historical
{
    public class Historic<T> : ITemporalValue<T>
    {
        public T Value { get; set; }
        public HistoricPeriod Period { get; set; }
    }
}