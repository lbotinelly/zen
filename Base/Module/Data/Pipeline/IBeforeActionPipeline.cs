namespace Zen.Base.Module.Data.Pipeline
{
    public interface IBeforeActionPipeline : IPipelinePrimitive
    {
        T Process<T>(string action, T current, T source) where T : Data<T>;
    }
}