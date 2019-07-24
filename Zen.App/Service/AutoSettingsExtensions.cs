﻿using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Zen.App.Provider;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.App.Service
{
    public static class AutoSettingsExtensions
    {
        internal static IServiceCollection ResolveSettingsPackage(this IServiceCollection serviceCollection)
        {
            var probe = Resolution.GetClassesByInterface<IAppOrchestrator<IZenPermission>>(false);
            var instance = probe.FirstOrDefault()?.CreateInstance<IAppOrchestrator<IZenPermission>>();

            serviceCollection.AddSingleton(s => instance);

            return serviceCollection;
        }
    }


}