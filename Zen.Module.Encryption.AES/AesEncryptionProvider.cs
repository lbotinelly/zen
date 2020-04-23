using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Zen.Base;
using Zen.Base.Common;
using Zen.Base.Module.Encryption;

namespace Zen.Module.Encryption.AES
{
    [Priority(Level = -1)]
    public class AesEncryptionProvider : EncryptionProviderPrimitive
    {
        private readonly RijndaelManaged _aesAlg;
        public AesEncryptionProvider(IOptions<Configuration.Options> options) : this(options.Value) { }

        public AesEncryptionProvider(Configuration.Options options)
        {
            try
            {
                Options = options;

                var rjKey = Options?.Key;
                var rjIv = Options?.InitializationVector;

                if (rjKey == null) throw new ArgumentNullException(nameof(Options.Key));
                if (rjIv == null) throw new ArgumentNullException(nameof(Options.InitializationVector));

                if (rjKey.Length != 32) throw new ArgumentOutOfRangeException(Options.Key, ConstantStrings.KeySizeException);
                if (rjIv.Length != 16) throw new ArgumentOutOfRangeException(Options.InitializationVector, ConstantStrings.VectorSizeException);

                _aesAlg = new RijndaelManaged
                {
                    Key = Encoding.ASCII.GetBytes(rjKey),
                    IV = Encoding.ASCII.GetBytes(rjIv)
                };
                OperationalStatus = EOperationalStatus.Operational;
            }
            catch (Exception e)
            {
                Current.Log.Add<AesEncryptionProvider>(e);
                OperationalStatus = EOperationalStatus.Error;
                throw;
            }
        }

        private Configuration.Options Options { get; }
        public override void Initialize() { }
        public override string Decrypt(string pContent)
        {
            if (pContent == null) return null;

            string plaintext;

            var base64Content = Convert.FromBase64String(pContent);

            using (var msDecrypt = new MemoryStream(base64Content))
            {
                var deCryptT = _aesAlg.CreateDecryptor(_aesAlg.Key, _aesAlg.IV);

                using (var csDecrypt = new CryptoStream(msDecrypt, deCryptT, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    plaintext = srDecrypt.ReadToEnd();
                }
            }

            return plaintext;
        }

        public override string Encrypt(string pContent)
        {
            using (var msEncrypt = new MemoryStream())
            {
                string ret;

                var enCryptT = _aesAlg.CreateEncryptor(_aesAlg.Key, _aesAlg.IV);

                using (var csEncrypt = new CryptoStream(msEncrypt, enCryptT, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(pContent);
                        swEncrypt.Flush();
                        csEncrypt.FlushFinalBlock();
                    }

                    ret = Convert.ToBase64String(msEncrypt.ToArray());
                }

                return ret;
            }
        }
    }
}