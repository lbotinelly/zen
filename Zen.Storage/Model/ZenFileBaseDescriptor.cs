using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Storage.Provider.File;

namespace Zen.Storage.Model
{
    public class ZenFileBaseDescriptor<T> : Data<T> where T : Data<T>
    {
        private readonly ZenFileDescriptorAttribute _attribute;
        private readonly ZenFileStoragePrimitive _provider;

        public ZenFileBaseDescriptor()
        {
            _attribute = GetType()
                             .GetCustomAttributes(typeof(ZenFileDescriptorAttribute), false)
                             .Select(i => (ZenFileDescriptorAttribute) i)
                             .FirstOrDefault() ?? new ZenFileDescriptorAttribute();
            _provider =
                _attribute.StorageType == null ? Current.FileStorageProvider : _attribute.StorageType.CreateInstance<ZenFileStoragePrimitive>();
        }

        public async Task<Stream> Fetch() { return await _provider.Fetch((IZenFileDescriptor) this); }

        public async Task<string> Store(Stream source) { return await _provider.Store((IZenFileDescriptor) this, source); }
        public async Task<bool> Exists() { return await _provider.Exists((IZenFileDescriptor) this); }
    }
}