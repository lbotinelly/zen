using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Media.Processing;
using Zen.Storage.Cache;
using Zen.Storage.Model;

namespace Zen.Web.Media
{
    [Route("api/media/storage")]
    public class MediaStorageController : ControllerBase
    {
        [HttpGet]
        public object Get([FromQuery] string id)
        {
            // First, prep.

            var query = Request.Query;
            var dictQuery = query
                .Where(i => i.Key != "id")
                .OrderBy(i => i.Key)
                .ToDictionary(i => i.Key, i => i.Value);

            var hasParameter = dictQuery.Count != 0;

            var cacheTag = (id + dictQuery.ToJson()).Sha512Hash();
            var mimeCacheTag = cacheTag + "_mimeType";

            // Is this configuration already cached? Fetch and reply.

            using var cachedStream = Local.Read(cacheTag);
            var cachedMimeType = Local.ReadString(mimeCacheTag);

            if (hasParameter)
                if (cachedStream != null && cachedMimeType != null)
                {
                    var cachedStreamContent = cachedStream.ToByteArray();

                    Log.KeyValuePair(id, cacheTag + " " + cachedMimeType);
                    return File(cachedStreamContent, cachedMimeType);
                }

            var file = ZenFile.Get(id);
            using var stream = file.Fetch().Result;

            var targetStreamMimeType = file.MimeType;

            if (!hasParameter) // No modifiers, so just return the fetched entry.
                return File(stream.ToByteArray(), targetStreamMimeType);

            var pipeline = Request.Query.ToRasterImagePipeline();

            pipeline.SourcePackage = stream.ToImagePackage();
            using var resultStream = pipeline.Process();

            // Save in cache.
            Local.Write(cacheTag, resultStream);
            Local.WriteString(mimeCacheTag, pipeline.Format.DefaultMimeType);
            Log.KeyValuePair(pipeline.Format.DefaultMimeType + " media cache ", mimeCacheTag);

            return File(resultStream.ToByteArray(), pipeline.Format.DefaultMimeType);
        }

        //[Route(""), HttpPost]
        //public async Task<HttpResponseMessage> Put()
        //{
        //    if (!Request.Content.IsMimeMultipartContent()) throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

        //    var mustCompress = false;

        //    var allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs();
        //    var targetFormat = allUrlKeyValues.LastOrDefault(x => x.Key == "targetFormat").Value;

        //    if (targetFormat != null) mustCompress = true;

        //    var provider = new MultipartMemoryStreamProvider();

        //    File o = null;

        //    try
        //    {

        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        foreach (var item in provider.Contents)
        //            if (item.Headers.ContentDisposition.FileName != null)
        //            {
        //                var entry = new MultipartEntry
        //                {
        //                    Name = item.Headers.ContentDisposition.FileName.Replace("\"", "").Replace("\"", ""),
        //                    Stream = item.ReadAsStreamAsync().Result
        //                };

        //                Current.Log.Add("File:" + entry.Name + ", size:" + entry.Stream.Length + " bytes");

        //                if (mustCompress) Current.Log.Add("    (will compress)");

        //                var stoStream = entry.Stream;

        //                // http://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c

        //                var mem = new MemoryStream();
        //                stoStream.CopyTo(mem);

        //                o = File.Load(mem, entry.Name);

        //            }
        //        return new SimpleHttpResponseMessage(HttpStatusCode.OK, new { id = o.Id, guid = o.Id, o.CreationTime, o.FileSize, o.MimeType, o.OriginalName, o.Tags }.ToJson());
        //    }
        //    catch (Exception e) { return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e); }
        //}

        //public class MultipartEntry
        //{
        //    public string Name { get; set; }
        //    public Stream Stream { get; set; }
        //}
    }
}