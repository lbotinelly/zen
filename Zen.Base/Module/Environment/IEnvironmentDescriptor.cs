namespace Zen.Base.Module.Environment
{
    public interface IEnvironmentDescriptor
    {
        string Name { get; }
        string Code { get; }
        int Value { get; }
        string ToString();
    }
}