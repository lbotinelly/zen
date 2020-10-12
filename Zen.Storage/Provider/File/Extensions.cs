using System.IO;
using System.Threading.Tasks;
using Zen.Base.Extension;

namespace Zen.Storage.Provider.File
{
    public static class Extensions
    {
        public static async Task<T> Get<T>(this IFileStorage repo, string path, string file)
        {
            var payload = await repo.GetStream(path, file);
            return payload.XmlToType<T>();
        }

        public static async Task<string> GetText(this IFileStorage repo, string path, string file)
        {
            var payload = await GetStream(repo, path, file);
            return payload?.ContentToString();
        }
        public static async Task<Stream> GetStream(this IFileStorage repo, string path, string file)
        {
            var contents = repo?.Collection(path)?.Result;
            if (contents?.ContainsKey(file) != true) return null;

            var payload = await repo.Fetch((IFileDescriptor)contents[file]);
            return payload;
        }
    }
}