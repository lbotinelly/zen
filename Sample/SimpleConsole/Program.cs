using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Data;
using Zen.Base.Module.Encryption;

namespace SimpleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Current.Log.Info("Hello World!");

            var sourceStr = "The California Consumer Privacy Act goes into effect January 1, 2020";

            var md5Hashed = sourceStr.Md5Hash();
            var Hash_Guid = sourceStr.HashGuid();
            var Sha512Hsh = sourceStr.Sha512Hash();
            var Meta_Hash = sourceStr.MetaHash();
            var To_Base64 = sourceStr.ToBase64();
            var encrypted = sourceStr.Encrypt();
            var decrypted = encrypted.Decrypt();


            Current.Log.Info($"sourceStr: {sourceStr}");
            Current.Log.Info($"md5Hashed: {md5Hashed}");
            Current.Log.Info($"Hash_Guid: {Hash_Guid}");
            Current.Log.Info($"Sha512Hsh: {Sha512Hsh}");
            Current.Log.Info($"Meta_Hash: {Meta_Hash}");
            Current.Log.Info($"To_Base64: {To_Base64}");
            Current.Log.Info($"encrypted: {encrypted}");
            Current.Log.Info($"decrypted: {decrypted}");

        }
    }

    [DataConfig]
    public class SampleModel : Data<SampleModel>
    {
        [Key]
        public string Id;
        [Display]
        public string Name;
    }
}
