namespace Zen.Base.Module.Encryption
{
    public static class Extensions
    {
        public static string Encrypt(this string source) { return Current.Encryption.TryEncrypt(source); }

        public static string Decrypt(this string encryptedSource) { return Current.Encryption.TryDecrypt(encryptedSource); }
    }
}