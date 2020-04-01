using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zen.Web.Communication.Push;
using Zen.Web.Model.Configuration;

namespace Zen.Web
{
    public static class Current
    {
        private static readonly Lazy<IHttpContextAccessor> ContextProvider = new Lazy<IHttpContextAccessor>(() => Base.Module.Service.Instances.ServiceProvider.GetService<IHttpContextAccessor>(), true);

        private static readonly Lazy<WebConfiguration> LazyConfiguration = new Lazy<WebConfiguration>(() => Base.Configuration.Options.GetSection("Web").Get<WebConfiguration>(), true);

        public static WebConfiguration Configuration = LazyConfiguration.Value;

        private static readonly Lazy<PushDispatcherPrimitive> PushDispatcherProvider = new Lazy<PushDispatcherPrimitive>(() => Base.Module.Service.Instances.ServiceProvider.GetService<PushDispatcherPrimitive>(), true);

        public static HttpContext Context => ContextProvider.Value?.HttpContext;
        public static PushDispatcherPrimitive PushDispatcher => PushDispatcherProvider.Value;
    }
}