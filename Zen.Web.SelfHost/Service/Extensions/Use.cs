using Microsoft.AspNetCore.Builder;

namespace Zen.Web.SelfHost.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWebSelfHost(this IApplicationBuilder app)
        {
            Current.SelfHostOrchestrator.Start();
        }
    }
}