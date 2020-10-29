using Zen.Base.Common;
using Zen.Base.Module.Encryption;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -2)]
    public class NullEncryptionProvider : EncryptionProviderPrimitive
    {
        public override string Decrypt(string pContent) { return pContent; }
        public override string Encrypt(string pContent) { return pContent; }
        public override void Initialize() { }
    }
}