using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Zen.Web.Auth.Service.Extensions
{
    public static class Use
    {
        public static void UseZenWebAuth(this IApplicationBuilder app, Action<IBuilder> configuration = null, IHostingEnvironment env = null)
        {
            configuration = configuration ?? (x => { });

            var optionsProvider = app.ApplicationServices.GetService<IOptions<Options>>();

            var options = new Options(optionsProvider.Value);

            var builder = new Builder(app, options);

            app
                .UseAuthentication();

            app.UseSession();

            configuration.Invoke(builder);
        }
    }
}