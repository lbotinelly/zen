using System.Collections.Concurrent;

namespace Zen.Base.Module.Data.Pipeline
{
    public interface IPipelinePrimitive
    {
        ConcurrentDictionary<string, object> Headers<T>() where T : Data<T>;
    }
}