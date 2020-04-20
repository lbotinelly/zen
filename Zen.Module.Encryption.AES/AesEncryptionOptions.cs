using Zen.Base;
using Zen.Base.Common;

namespace Zen.Module.Encryption.AES
{
    [Priority(Level = -1)]
    public class AesEncryptionOptions
    {
        public string Key { get; set; } = Strings.default_aes_key;
        public string InitializationVector { get; set; } = Strings.default_aes_vector;
    }
}