using Microsoft.AspNetCore.Builder;
using System.Threading;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWebAuth(this IApplicationBuilder app)
        {
            Web.Instances.BeforeUseEndpoints.Add(() =>
            {

                app.UseAuthentication();
                app.UseAuthorization();
                Base.Log.KeyValuePair("Auth", "['UseAuthentication','UseAuthorization']");

            });
        }
    }
}