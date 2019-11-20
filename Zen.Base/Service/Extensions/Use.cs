using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

namespace Zen.Base.Service.Extensions
{
    public static class Use
    {
        public static IApplicationBuilder UseZen(this IApplicationBuilder app, Action<IZenBuilder> configuration = null, IHostEnvironment env = null)
        {
            configuration = configuration ?? (x => { });

            Instances.ApplicationBuilder = app;

            var optionsProvider = app.ApplicationServices.GetService<IOptions<ZenOptions>>();

            var options = new ZenOptions(optionsProvider.Value);

            AutoService.UseAll(app, env);

            var builder = new ZenBuilder(app, options);

            configuration.Invoke(builder);

            Current.State = Status.EState.Running;

            Current.Log.Add(Current.State.ToString(), Message.EContentType.StartupSequence, Host.ApplicationAssemblyName);

            return app;
        }
    }
}