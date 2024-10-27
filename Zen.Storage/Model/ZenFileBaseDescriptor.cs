﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Storage.Provider.File;
using static Zen.Storage.Provider.File.IFileStorage;

namespace Zen.Storage.Model
{
    public class ZenFileBaseDescriptor<T> : Data<T> where T : Data<T>
    {
        private readonly ZenFileDescriptorAttribute _attribute;
        private readonly FileStoragePrimitive _provider;

        public ZenFileBaseDescriptor()
        {
            _attribute = GetType()
                             .GetCustomAttributes(typeof(ZenFileDescriptorAttribute), false)
                             .Select(i => (ZenFileDescriptorAttribute) i)
                             .FirstOrDefault() ?? new ZenFileDescriptorAttribute();
            _provider =
                _attribute.StorageType == null ? Current.FileStorageProvider : _attribute.StorageType.CreateInstance<FileStoragePrimitive>();
        }

        public async Task<Stream> Fetch() { return await _provider.Fetch((IFileDescriptor)this); }
        public async Task<string> GetPath() { return await _provider.GetPath((IFileDescriptor)this); }

        public async Task<StoreResult> Store(Stream source) { return await _provider.Store((IFileDescriptor) this, source); }
        public async Task<bool> Exists() { return await _provider.Exists((IFileDescriptor) this); }
    }
}