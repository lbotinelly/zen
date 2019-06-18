namespace Zen.Base.Module.Data.Pipeline
{
    public interface IBeforeActionPipeline : IPipelinePrimitive
    {
        T Process<T>(EActionType type, EActionScope scope, T current, T source) where T : Data<T>;
    }
}