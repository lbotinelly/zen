namespace Zen.Base.Module.Data.Pipeline
{
    public interface IAfterActionPipeline : IPipelinePrimitive
    {
        void Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>;
    }
}