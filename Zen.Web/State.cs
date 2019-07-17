using Microsoft.AspNetCore.Http;
using Zen.Base.Process;

namespace Zen.Web
{
    public static class State
    {
        private static string _httpContextKey = "_httpContextKey";
        public static IHttpContextAccessor Context { get => ThreadContext.Get<IHttpContextAccessor>(_httpContextKey); set => ThreadContext.Set(_httpContextKey, value); }
    }
}