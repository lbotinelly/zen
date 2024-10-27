using System.Collections.Generic;

namespace Zen.Base.Module.Data.Pipeline
{
    public interface IBeforeActionPipeline : IPipelinePrimitive
    {
        T Process<T>(EActionType type, EActionScope scope, Mutator mutator, T current, T source) where T : Data<T>;
        KeyValuePair<string, string>? ParseRequest<T>(Dictionary<string, List<string>> requestData) where T : Data<T>;
    }
}