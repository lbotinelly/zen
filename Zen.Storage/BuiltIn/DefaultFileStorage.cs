using System.IO;
using System.Threading.Tasks;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Storage.Provider.File;

namespace Zen.Storage.BuiltIn
{
    [Priority(Level = -99)]
    public class DefaultFileStorage : FileStoragePrimitive
    {
        private string _location;

        public override IFileStorage ResolveStorage()
        {
            _location = Host.DataDirectory;
            return this;
        }

        public override async Task<Stream> Fetch(IFileDescriptor fileDescriptor)
        {
            var targetPath = Path.Combine(fileDescriptor.StoragePath, fileDescriptor.StorageName);
            return new FileStream(targetPath, FileMode.Open);
        }

        public override async Task<string> Store(IFileDescriptor definition, Stream source)
        {
            var targetPath = Path.Combine(definition.StoragePath, definition.StorageName);

            using (Stream file = File.Create(targetPath)) { source.CopyStreamTo(file); }

            return definition.Locator;
        }
    }
}