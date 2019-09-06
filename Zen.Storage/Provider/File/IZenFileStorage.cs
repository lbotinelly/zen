using System.IO;
using System.Threading.Tasks;
using Zen.Base.Common;

namespace Zen.Storage.Provider.File
{
    public interface IZenFileStorage : IZenProvider
    {
        void ResolveStorage();
        Task<Stream> Fetch(IZenFileDescriptor definition);
        Task<string> Store(IZenFileDescriptor definition, Stream source);
        Task<bool> Exists(IZenFileDescriptor definition);
    }
}