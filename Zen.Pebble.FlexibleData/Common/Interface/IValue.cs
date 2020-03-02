namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IValue<out T>
    {
        T Value { get; }
    }
}