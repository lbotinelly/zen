using System.IO;
using System.Threading.Tasks;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Storage.Provider.File;

namespace Zen.Storage.BuiltIn
{
    [Priority(Level = -99)]
    public class DefaultFileStorage : ZenFileStoragePrimitive
    {
        private string _location;

        public override void ResolveStorage() { _location = Host.DataDirectory; }

        public override async Task<Stream> Fetch(IZenFileDescriptor fileDescriptor)
        {
            var targetPath = Path.Combine(fileDescriptor.StoragePath, fileDescriptor.StorageName);
            return new FileStream(targetPath, FileMode.Open);
        }

        public override async Task<string> Store(IZenFileDescriptor definition, Stream source)
        {
            var targetPath = Path.Combine(definition.StoragePath, definition.StorageName);

            using (Stream file = File.Create(targetPath)) { source.CopyStreamTo(file); }

            return definition.Locator;
        }
    }
}