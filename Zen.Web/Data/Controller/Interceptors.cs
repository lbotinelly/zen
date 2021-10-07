using System.Collections.Generic;
using System.Linq;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.Web.Data.Controller.Contracts;

namespace Zen.Web.Data.Controller
{
    public static class Interceptors
    {
        internal static readonly List<IDataControllerPostFetchInterceptor> DataControllerPostFetchInterceptors = IoC.GetClassesByInterface<IDataControllerPostFetchInterceptor>(false).CreateInstances<IDataControllerPostFetchInterceptor>().ToList();
    }
}