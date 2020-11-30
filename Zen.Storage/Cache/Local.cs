using System.IO;
using System.Text;
using Zen.Base;

namespace Zen.Storage.Cache
{
    public static class Local
    {
        public static readonly string BasePath = $"{Host.DataDirectory}{Path.DirectorySeparatorChar}cache{Path.DirectorySeparatorChar}";

        public static string WriteString(string key, string source) { return Write(key, new MemoryStream(Encoding.UTF8.GetBytes(source))); }

        public static string ReadString(string key)
        {
            using var stream = Read(key);
            return stream == null ? null : new StreamReader(stream).ReadToEnd();
        }

        public static string Write(string key, Stream source)
        {
            var targetFile = Path.Combine(BasePath, key);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile));

            using var fileStream = File.Create(targetFile);
            source.Seek(0, SeekOrigin.Begin);
            source.CopyTo(fileStream);
            fileStream.Close();

            source.Seek(0, SeekOrigin.Begin);

            return targetFile;
        }

        public static Stream Read(string key)
        {
            var path = BasePath + key;

            return !File.Exists(path) ? null : File.OpenRead(path);
        }
    }
}