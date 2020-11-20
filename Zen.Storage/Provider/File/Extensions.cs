using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static async Task<dynamic> GetDynamic(this IFileStorage repo, string path, string file)
        {
            var content = await repo.GetText(path, file);

            var doc = new XmlDocument();
            doc.LoadXml(content);

            var baseObj = JObject.Parse(JsonConvert.SerializeXmlNode(doc));

            dynamic result = baseObj;
            return result;
        }

        public static async Task<string> GetText(this IFileStorage repo, string path, string file)
        {
            await using var payload = await GetStream(repo, path, file);
            return payload?.ContentToString();
        }
        public static async void StoreText(this IFileStorage repo, string path, string file, string contents)
        {
            await StoreStream(repo, path, file, new MemoryStream(Encoding.ASCII.GetBytes(contents)));
        }
        public static async Task<Stream> GetStream(this IFileStorage repo, string path, string file)
        {
            var contents = repo?.Collection(path)?.Result;
            if (contents?.ContainsKey(file) != true) return null;

            var payload = await repo.Fetch((IFileDescriptor)contents[file]);
            return payload;
        }
        public static async Task StoreStream(this IFileStorage repo, string path, string file, Stream stream)
        {
            var fileDescriptor = new DefaultFileDescriptor { StorageName = file, StoragePath = path };
            if (repo != null) await repo.Store(fileDescriptor, stream);
        }
    }
}