using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Web
{
    public static class Current
    {
   
        private static readonly Lazy<IHttpContextAccessor> ContextProvider = new Lazy<IHttpContextAccessor>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IHttpContextAccessor>(), true);
        public static HttpContext Context => ContextProvider.Value?.HttpContext;
        public readonly static Configuration.IOptions Options = Base.Configuration.GetSettings<Configuration.IOptions, Configuration.Options>(new Configuration.Options(), "Web");
    }
}