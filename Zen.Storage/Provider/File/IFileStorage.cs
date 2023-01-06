using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zen.Base.Common;

namespace Zen.Storage.Provider.File
{
    public interface IFileStorage : IZenProvider
    {

        public class StoreResult
        {
            public string Name { get; set; }
            public Dictionary<string, string> Metadata { get; set; }
        }

        IFileStorage ResolveStorage();
        Task<Stream> Fetch(IFileDescriptor definition);
        Task<StoreResult> Store(IFileDescriptor definition, Stream source);
        Task<bool> Exists(IFileDescriptor definition);
        Task<Dictionary<string, IStorageEntityDescriptor>> Collection(string referencePath = null);
    }
}