using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;

namespace Zen.Web
{
    public static class Current
    {
        private static readonly Lazy<IHttpContextAccessor> ContextProvider = new Lazy<IHttpContextAccessor>(() => Instances.ServiceProvider.GetService<IHttpContextAccessor>(), true);
        public static HttpContext Context => ContextProvider.Value?.HttpContext;
    }
}