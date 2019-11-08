using System.Collections.Generic;

namespace Zen.Base.Module.Data.Pipeline
{
    public interface IPipelinePrimitive
    {
        string PipelineName { get; }
        Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, Microsoft.Extensions.Primitives.StringValues> requestHeaders) where T : Data<T>;
    }
}