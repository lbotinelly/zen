using System;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Module.Service;
using Zen.Storage.Provider.Configuration;
using Zen.Storage.Provider.File;

namespace Zen.Storage
{
    public static class Current
    {
        private static readonly Lazy<ConfigurationStorage> ConfigurationStorage = new Lazy<ConfigurationStorage>(() => Instances.ServiceProvider.GetService<ConfigurationStorage>(), true);
        public static IConfigurationStorage Configuration = ConfigurationStorage.Value;

        private static readonly Lazy<FileStoragePrimitive> FileStorage = new Lazy<FileStoragePrimitive>(() => Instances.ServiceProvider.GetService<FileStoragePrimitive>(), true);
        public static FileStoragePrimitive FileStorageProvider = FileStorage.Value;

        //private static readonly Lazy<IZenFileDescriptor> LazyFileDescriptor = new Lazy<IZenFileDescriptor>(() => Instances.ServiceProvider.GetService<IZenFileDescriptor>(), true);
        //public static IZenFileDescriptor FileDescriptor = LazyFileDescriptor.Value;
    }
}