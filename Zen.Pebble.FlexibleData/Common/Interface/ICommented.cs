namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface ICommented<out T>: IValue<T>
    {
        string Comments { get; }
    }
}