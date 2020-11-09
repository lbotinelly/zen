using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zen.Base.Common;

namespace Zen.Base.Module.Service
{
    public static class AutoService
    {
        public static List<IZenAutoAddService> AddQueue { get; internal set; } = IoC.GetInstances<IZenAutoAddService>(false);
        public static List<IZenAutoUseService> UseQueue { get; internal set; } = IoC.GetInstances<IZenAutoUseService>(false);

        public static void Add()
        {
            foreach (var item in AddQueue)

                try
                {
                    Instances.ServiceCollection.Add(item);

                    // Building the provider incrementally as the modules are loaded and made available.
                    Instances.ServiceProvider = Instances.ServiceCollection.BuildServiceProvider();


                }
                catch (Exception e)
                {
                    
                    // throw new InvalidDataException("Error initializing " + item.GetType().FullName, e);
                }

            var zenServices = Instances.ServiceCollection.Where(i => typeof(IZenProvider).IsAssignableFrom(i.ServiceType)).ToList();

            // Instances.ServiceProvider = Instances.ServiceCollection.BuildServiceProvider();

            foreach (var zenService in zenServices)
                try { ((IZenProvider) Instances.ServiceProvider.GetService(zenService.ServiceType)).Initialize(); } catch (Exception e) { Current.Log.Add(e, zenService.ServiceType.FullName); }
        }

        public static void UseAll(IApplicationBuilder app, IHostEnvironment env)
        {
            foreach (var item in UseQueue) item.Use(app, env);
        }
    }
}