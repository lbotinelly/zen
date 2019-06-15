using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module;
using Zen.Base.Module.Encryption;

namespace SimpleConsole
{
    internal class Program
    {
        private static void Main(string[] args)
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

            var a = SampleModel.All().ToList();

            Current.Log.Info($"SampleModel have: {a.Count} records");

            new SampleModel {Id = Guid.NewGuid().ToString(), Name = "Blah"}.Save();
        }
    }

    public class SampleModel : Data<SampleModel>
    {
        [Display]
        public string Name;
        [Key]
        // ReSharper disable once InconsistentNaming
        public string Id { get; set; }
    }
}