using System;
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
            _location = Path.Combine(Host.DataDirectory, "storage");
            return this;
        }

        public override async Task<Stream> Fetch(IFileDescriptor definition)
        {
            var targetPath = _location;

            if (definition.StoragePath != null)
                targetPath = Path.Combine(_location, definition.StoragePath);

            targetPath = Path.Combine(targetPath, definition.StorageName);

            return new FileStream(targetPath, FileMode.Open);
        }

        public override async Task<string> Store(IFileDescriptor definition, Stream source)
        {
            var targetPath = _location;

            if (definition.StoragePath != null)
                targetPath = Path.Combine(_location, definition.StoragePath);

            Directory.CreateDirectory(targetPath);

            targetPath = Path.Combine(targetPath, definition.StorageName);

            try
            {
                using (Stream file = File.Create(targetPath))
                {
                    source.CopyStreamTo(file);
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            return definition.Locator;
        }
    }
}