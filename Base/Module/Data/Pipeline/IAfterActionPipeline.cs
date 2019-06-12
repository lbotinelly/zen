namespace Zen.Base.Module.Data.Pipeline
{
    public interface IAfterActionPipeline : IPipelinePrimitive
    {
        void Process<T>(string action, T current, T source) where T : Data<T>;
    }
}