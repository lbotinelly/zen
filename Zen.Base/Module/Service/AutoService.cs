using System;
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
        public static List<IZenAutoAddService> AddQueue { get; internal set; } = Resolution.GetInstances<IZenAutoAddService>(false);
        public static List<IZenAutoUseService> UseQueue { get; internal set; } = Resolution.GetInstances<IZenAutoUseService>(false);

        public static void Add()
        {
            foreach (var item in AddQueue) item.Add(Instances.ServiceCollection);

            var zenServices = Instances.ServiceCollection.Where(i => typeof(IZenProvider).IsAssignableFrom(i.ServiceType)).ToList();

            Instances.ServiceProvider = Instances.ServiceCollection.BuildServiceProvider();

            foreach (var zenService in zenServices)
                try { ((IZenProvider)Instances.ServiceProvider.GetService(zenService.ServiceType)).Initialize(); } catch (Exception e) { Current.Log.Add(e, zenService.ServiceType.FullName); }
        }

        public static void UseAll(IApplicationBuilder app, IHostingEnvironment env)
        {
            foreach (var item in UseQueue) item.Use(app, env);
        }
    }
}