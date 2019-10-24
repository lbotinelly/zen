using System.IO;
using System.Text;
using Zen.Base;

namespace Zen.Storage.Cache
{
    public static class Local
    {
        internal static readonly string BasePath = $"{Host.DataDirectory}{Path.DirectorySeparatorChar}cache{Path.DirectorySeparatorChar}";

        public static bool WriteString(string key, string source) { return Write(key, new MemoryStream(Encoding.UTF8.GetBytes(source))); }

        public static string ReadString(string key)
        {
            var stream = Read(key);
            return stream == null ? null : new StreamReader(stream).ReadToEnd();
        }

        public static bool Write(string key, Stream source)
        {
            Directory.CreateDirectory(BasePath);
            var fileStream = File.Create(BasePath + key);
            source.Seek(0, SeekOrigin.Begin);
            source.CopyTo(fileStream);
            fileStream.Close();
            source.Seek(0, SeekOrigin.Begin);

            return true;
        }

        public static Stream Read(string key)
        {
            var path = BasePath + key;

            return !File.Exists(path) ? null : File.OpenRead(path);
        }
    }
}