using System.IO;
using System.Threading.Tasks;
using Zen.Base.Common;

namespace Zen.Storage.Provider.File
{
    public interface IFileStorage : IZenProvider
    {
        void ResolveStorage();
        Task<Stream> Fetch(IFileDescriptor definition);
        Task<string> Store(IFileDescriptor definition, Stream source);
        Task<bool> Exists(IFileDescriptor definition);
    }
}