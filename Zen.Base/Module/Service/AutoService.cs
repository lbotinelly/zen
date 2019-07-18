using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;

namespace Zen.Base.Module.Service
{
    public static class AutoService
    {
        public static List<IZenAutoAddService> AddQueue { get; internal set; } = Resolution.GetInstances<IZenAutoAddService>();
        public static List<IZenAutoUseService> UseQueue { get; internal set; } = Resolution.GetInstances<IZenAutoUseService>();

        public static void Add()
        {
            foreach (var item in AddQueue) item.Add(Instances.ServiceCollection);

            var zenServices = Instances.ServiceCollection.Where(i => typeof(IZenProvider).IsAssignableFrom(i.ServiceType)).ToList();

            Instances.ServiceProvider = Instances.ServiceCollection.BuildServiceProvider();

            foreach (var zenService in zenServices) ((IZenProvider)Instances.ServiceProvider.GetService(zenService.ServiceType)).Initialize();

        }

        public static void UseAll(IApplicationBuilder app, IHostingEnvironment env)
        {

            foreach (var item in UseQueue) item.Use(app, env);
        }
    }
}