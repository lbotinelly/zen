using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Zen.Base.Service.Extensions
{
    public static class Use
    {
        public static IApplicationBuilder UseZen(this IApplicationBuilder app, Action<IZenBuilder> configuration = null, IHostingEnvironment env = null)
        {

            configuration = configuration ?? new Action<IZenBuilder>(x => { });

            Module.Service.Instances.ApplicationBuilder = app;

            var optionsProvider = app.ApplicationServices.GetService<IOptions<ZenOptions>>();

            var options = new ZenOptions(optionsProvider.Value);

            foreach (var item in Module.Service.Instances.AutoZenServices)
            {
                item.Use(app, env);
            }

            var builder = new ZenBuilder(app, options);

            configuration.Invoke(builder);

            Current.Log.Add(Current.State.ToString());

            return app;
        }
    }
}