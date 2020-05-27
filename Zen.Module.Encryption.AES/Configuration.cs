using Microsoft.Extensions.Options;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Module.Encryption.AES
{
    public class Configuration : IConfigureOptions<Configuration.Options>
    {
        private readonly IOptions _options;

        public Configuration(IOptions<Options> options) => _options = options.Value;

        public void Configure(Options options)
        {
            options.Key = _options.Key;
            options.InitializationVector = _options.InitializationVector;
        }

        public interface IOptions
        {
            string Key { get; }
            string InitializationVector { get; }
        }

        [IoCIgnore]
        public class Options : IOptions
        {
            public string Key { get; set; }
            public string InitializationVector { get; set; }
        }

        [Priority(Level = -99)]
        public class AutoOptions : IOptions // If nothing else is defined, AutoOptions kicks in.
        {
            public string Key { get; } = "2314x# n07h1n9 15 und32 c0n7201#";
            public string InitializationVector { get; } = "57111 y0u2 w4732";
        }

    }
}