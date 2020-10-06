using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zen.Base.Common;

namespace Zen.Storage.Provider.File
{
    public abstract class FileStoragePrimitive : IFileStorage
    {
        public EOperationalStatus OperationalStatus { get; set; } = EOperationalStatus.Undefined;

        public void Initialize()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            ResolveStorage();
        }
        public virtual string Name { get; }
        public virtual string GetState() => $"{OperationalStatus}";

        public virtual IFileStorage ResolveStorage() { return this; }
        public virtual Task<Stream> Fetch(IFileDescriptor definition) => null;
        public virtual Task<string> Store(IFileDescriptor definition, Stream source) => null;
        public virtual Task<bool> Exists(IFileDescriptor definition) => Task.FromResult(false);
        public virtual Task<Dictionary<string, IStorageEntityDescriptor>> Collection(string referencePath = null) => null;
    }
}