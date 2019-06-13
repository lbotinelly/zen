using System;
using System.Collections.Generic;
using System.Text;

namespace Zen.Base.Module.Encryption
{
    public static class Extensions
    {
        public static string Encrypt(this string source) => Current.Encryption.TryEncrypt(source);
        public static string Decrypt(this string encryptedSource) => Current.Encryption.TryDecrypt(encryptedSource);
    }
}
