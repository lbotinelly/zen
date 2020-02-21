using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zen.Storage.Provider.File
{
    public abstract class FileStoragePrimitive : IFileStorage
    {
        public void Initialize()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            ResolveStorage();
        }

        public virtual void ResolveStorage() { }
        public virtual Task<Stream> Fetch(IFileDescriptor definition) => null;
        public virtual Task<string> Store(IFileDescriptor definition, Stream source) => null;
        public virtual Task<bool> Exists(IFileDescriptor definition) => Task.FromResult(false);
    }
}