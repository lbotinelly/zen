namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IBoundary<out T>
    {
        T Start { get; }
        T End { get; }
    }
}