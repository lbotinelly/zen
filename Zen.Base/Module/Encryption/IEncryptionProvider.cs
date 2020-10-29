using Zen.Base.Common;

namespace Zen.Base.Module.Encryption
{
    public interface IEncryptionProvider : IZenProvider
    {
        string Decrypt(string pContent);
        string Encrypt(string pContent);
        string TryDecrypt(string pContent);
        string TryEncrypt(string pContent);
    }
}