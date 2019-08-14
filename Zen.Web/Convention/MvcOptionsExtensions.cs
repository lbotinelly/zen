using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Zen.Web.Convention {
    public static class MvcOptionsExtensions
    {
        public static void UseCentralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Insert(0, new RouteConvention(routeAttribute));
        }
    }
}