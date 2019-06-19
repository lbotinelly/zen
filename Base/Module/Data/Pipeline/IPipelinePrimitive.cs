using System.Collections.Generic;

namespace Zen.Base.Module.Data.Pipeline
{
    public interface IPipelinePrimitive
    {
        Dictionary<string, object> Headers<T>() where T : Data<T>;
    }
}