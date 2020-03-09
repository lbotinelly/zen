using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.String.Localization.Concrete;

namespace Zen.Pebble.FlexibleData.String.Localization.Interface
{
    public interface ITemporalCommented<out T> : ITemporalValue<T>, ICommented<T> { }
    public class TemporalCommented<T> : ITemporalCommented<T>
    {
        public T Value { get; set; }
        public HistoricDateTimeBoundary Boundaries { get; set; }
        public string Comments { get; set; }
    }
}