using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Service;

namespace Zen.MessageQueue
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
            int HttpPort { get; set; }
            int HttpsPort { get; set; }
        }

        [IoCIgnore]
        public class Options : AutoOptions { }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public int HttpPort { get; set; } = 5000;
            public int HttpsPort { get; set; } = 5001;
        }
    }
}