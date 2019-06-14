namespace Zen.Base.Module.Data.Pipeline
{
    public interface IAfterActionPipeline : IPipelinePrimitive
    {
        void Process<T>(EAction action, T current, T source) where T : Data<T>;
    }
}