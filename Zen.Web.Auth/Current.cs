using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Web.Auth.Provider;

namespace Zen.Web.Auth
{
    public static class Current
    {
        private static readonly Lazy<IAuthPrimitive> InternalAuthProvider = new Lazy<IAuthPrimitive>(() => Instances.ServiceProvider.GetService<IAuthPrimitive>(), true);
        public static IAuthPrimitive AuthProvider = InternalAuthProvider.Value;
    }
}