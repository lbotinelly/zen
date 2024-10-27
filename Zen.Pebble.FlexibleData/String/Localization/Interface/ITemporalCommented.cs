using Zen.Pebble.FlexibleData.Common.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Interface
{
    public interface ITemporalCommented<out T> : ITemporalValue<T>, ICommented<T> { }
}