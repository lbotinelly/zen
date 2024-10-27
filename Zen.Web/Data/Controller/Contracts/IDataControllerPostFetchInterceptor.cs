using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Zen.Web.Data.Controller.Contracts
{
    public interface IDataControllerPostFetchInterceptor
    {
        List<JObject> HandleCollection(List<JObject> collection, Microsoft.AspNetCore.Http.HttpRequest request);
    }
}
