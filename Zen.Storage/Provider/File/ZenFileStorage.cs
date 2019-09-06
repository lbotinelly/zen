using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zen.Storage.Provider.File
{
    public abstract class ZenFileStoragePrimitive : IZenFileStorage
    {
        internal List<ZenFileStorageAttribute> attributes;

        public void Initialize()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            ResolveStorage();
        }

        public virtual void ResolveStorage() { }
        public virtual Task<Stream> Fetch(IZenFileDescriptor definition) => null;
        public virtual Task<string> Store(IZenFileDescriptor definition, Stream source) => null;
        public virtual Task<bool> Exists(IZenFileDescriptor definition) => Task.FromResult(false);
    }
}