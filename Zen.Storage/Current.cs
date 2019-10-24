using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Storage.Provider.Configuration;
using Zen.Storage.Provider.File;

namespace Zen.Storage
{
    public static class Current
    {
        private static readonly Lazy<ZenConfigurationStorage> ConfigurationStorage = new Lazy<ZenConfigurationStorage>(() => Instances.ServiceProvider.GetService<ZenConfigurationStorage>(), true);
        public static IZenConfigurationStorage Configuration = ConfigurationStorage.Value;

        private static readonly Lazy<ZenFileStoragePrimitive> FileStorage = new Lazy<ZenFileStoragePrimitive>(() => Instances.ServiceProvider.GetService<ZenFileStoragePrimitive>(), true);
        public static ZenFileStoragePrimitive FileStorageProvider = FileStorage.Value;

        //private static readonly Lazy<IZenFileDescriptor> LazyFileDescriptor = new Lazy<IZenFileDescriptor>(() => Instances.ServiceProvider.GetService<IZenFileDescriptor>(), true);
        //public static IZenFileDescriptor FileDescriptor = LazyFileDescriptor.Value;
    }
}