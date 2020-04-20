using Zen.Base.Common;
using Zen.Base.Module.Encryption;

namespace Zen.Base.Module.Default
{
    [Priority(Level = -2)]
    public class NullEncryptionProvider : EncryptionProviderPrimitive
    {
        public override string Decrypt(string pContent) { return pContent; }

        public override string Encrypt(string pContent) { return pContent; }

        public void Shutdown() { }
        public override void Initialize() { Events.ShutdownSequence.Actions.Add(Shutdown); }
    }
}