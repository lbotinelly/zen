using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Storage.Provider.Configuration;

namespace Zen.Storage
{
    public static class Current
    {
        private static readonly Lazy<ZenConfigurationStorage> ConfigurationStorage = new Lazy<ZenConfigurationStorage>(() => Instances.ServiceProvider.GetService<ZenConfigurationStorage>(), true);
        public static IZenConfigurationStorage Configuration = ConfigurationStorage.Value;
    }
}