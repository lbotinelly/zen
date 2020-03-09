namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IScoped<out TU, out T> : IValue<T>
    {
        TU Scope { get; }
    }
}