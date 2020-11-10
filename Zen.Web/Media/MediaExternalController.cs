using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Storage.Cache;

namespace Zen.Web.Media
{
    [Route("api/media/external")]
    public class MediaExternalController : ControllerBase
    {
        [HttpGet("get")]
        [ResponseCache(Duration = 1 * 24 * 60 * 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public object Get()
        {
            var query = Request.Query;
            if (!query.ContainsKey("url")) return null;

            var dictQuery = query.ToDictionary(i => i.Key, i => i.Value).OrderBy(i => i.Key);

            var cacheTag = dictQuery.ToJson().Sha512Hash();
            var mimeCacheTag = cacheTag + "_mimeType";

            var targetStream = Local.Read(cacheTag);
            var targetStreamMimeType = Local.ReadString(mimeCacheTag);

            if (targetStream != null && targetStreamMimeType != null)
            {
                Log.KeyValuePair(cacheTag + " " + targetStreamMimeType, query["url"]);
                return File(targetStream, targetStreamMimeType);
            }

            var pipeline = Request.Query.ToRasterImagePipeline();

            pipeline.SourcePackage = External.FetchImagePackage();
            var resultStream = pipeline.Process();

            Local.Write(cacheTag, resultStream);
            Local.WriteString(mimeCacheTag, pipeline.Format.DefaultMimeType);

            return File(resultStream, pipeline.Format.DefaultMimeType);
        }
    }
}