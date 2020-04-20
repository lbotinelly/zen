using Microsoft.Extensions.Options;
using Zen.Base.Common;

namespace Zen.Module.Encryption.AES
{
    public class AesEncryptionConfiguration : IConfigureOptions<AesEncryptionConfiguration.Options>
    {
        private readonly IOptions _options;

        public AesEncryptionConfiguration(IOptions<Options> options) => _options = options.Value;

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

        [Priority(Level = -1)]
        public class Options : IOptions
        {
            public string Key { get; internal set; }
            public string InitializationVector { get; internal set; }
        }
    }
}