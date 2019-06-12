using Zen.Base.Common;
using Zen.Base.Module.Encryption;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -2)]
    public class NullEncryptionProvider : IEncryptionProvider
    {
        public void Configure(params string[] oParms) { }
        public string Decrypt(string pContent) => pContent;
        public string Encrypt(string pContent) => pContent;
        public void Shutdown() { }
    }
}