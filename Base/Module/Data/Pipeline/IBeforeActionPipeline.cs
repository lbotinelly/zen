namespace Zen.Base.Module.Data.Pipeline
{
    public interface IBeforeActionPipeline : IPipelinePrimitive
    {
        T Process<T>(EAction action, T current, T source) where T : Data<T>;
    }
}