using Microsoft.AspNetCore.Builder;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWebAuth(this IApplicationBuilder app)
        {
            Web.Instances.BeforeUseEndpoints.Add(() => app.UseAuthentication());
            Web.Instances.BeforeUseEndpoints.Add(() => app.UseAuthorization());

            app.UseCookiePolicy();
            app.UseSession();
        }
    }
}