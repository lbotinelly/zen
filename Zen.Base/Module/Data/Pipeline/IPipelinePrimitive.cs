using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Zen.Base.Module.Data.Pipeline
{
    public interface IPipelinePrimitive
    {
        string PipelineName { get; }
        Dictionary<string, object> Headers<T>(ref DataAccessControl accessControl, Dictionary<string, StringValues> requestHeaders, EActionScope scope, T model) where T : Data<T>;
    }
}