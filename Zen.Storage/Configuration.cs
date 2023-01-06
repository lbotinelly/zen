using Microsoft.Extensions.Options;
using System.IO;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.Storage
{
    public class Configuration : IConfigureOptions<Configuration.IOptions>
    {

        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(IOptions options)
        {
            _options.CopyMembersTo(options);
        }

        public interface IOptions
        {
            FileOptions File { get; set; }
        }

        public class FileOptions
        {
            public string StoragePath { get; set; }
        }


        [IoCIgnore]
        public class Options : AutoOptions { }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public AutoOptions()
            {
                File = new FileOptions() { StoragePath = Path.Combine(Host.DataDirectory, "storage") };
            }


            public FileOptions File { get; set; }
        }
    }
}