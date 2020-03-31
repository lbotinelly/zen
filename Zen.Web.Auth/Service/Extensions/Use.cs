using Microsoft.AspNetCore.Builder;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWebAuth(this IApplicationBuilder app)
        {
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
        }
    }
}