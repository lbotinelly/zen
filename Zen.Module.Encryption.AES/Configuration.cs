using Microsoft.Extensions.Options;
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
    }
}