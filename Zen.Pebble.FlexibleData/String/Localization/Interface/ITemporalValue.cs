using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.String.Localization.Concrete;

namespace Zen.Pebble.FlexibleData.String.Localization.Interface
{
    public interface ITemporalValue<out T> : IValue<T>
    {
        HistoricPeriod Period { get; }
    }
}