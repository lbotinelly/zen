namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface ICultured<out T> : IValue<T>
    {
        string Culture { get; }
    }
}