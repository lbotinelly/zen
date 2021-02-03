using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Media.Processing;
using Zen.Storage.Cache;
using Zen.Storage.Model;

namespace Zen.Web.Media
{
    [Route("api/media/storage")]
    public class MediaStorageController : ControllerBase
    {
        [ResponseCache(Duration = int.MaxValue, Location = ResponseCacheLocation.Any, NoStore = false)]
        [HttpGet]
        public object Get([FromQuery] string id)
        {
            // First, prep.

            Log.Add<MediaStorageController>("FETCH " + id);

            var query = Request.Query;
            var dictQuery = query
                .Where(i => i.Key != "id")
                .OrderBy(i => i.Key)
                .ToDictionary(i => i.Key, i => i.Value);

            var hasParameter = dictQuery.Count != 0;

            DateTimeOffset offset;

            var idPayload = id + dictQuery.ToJson();

            var cacheTag = idPayload.Sha512Hash();
            var mimeCacheTag = cacheTag + "_mimeType";

            var entityTag = new EntityTagHeaderValue($"\"{cacheTag}\"");

            // Is this configuration already cached? Fetch and reply.

            using var cachedStream = Local.Read(cacheTag);
            var cachedMimeType = Local.ReadString(mimeCacheTag);

            if (hasParameter)
                if (cachedStream != null && cachedMimeType != null)
                {
                    var cachedStreamContent = cachedStream.ToByteArray();
                    offset = DateTime.MinValue;

                    Log.KeyValuePair(id, cacheTag + " " + cachedMimeType);
                    return File(cachedStreamContent, cachedMimeType, offset, entityTag);
                }

            var file = ZenFile.Get(id);

            if (file == null) return NotFound();

            if (!file.Exists().Result) return NotFound();

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

            offset = file.Creation;

            return File(resultStream.ToByteArray(), pipeline.Format.DefaultMimeType, offset, entityTag);
        }

        [Route("")]
        [HttpPost]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public List<string> Put()
        {
            var files = Request.Form.Files;

            var mediaObjects = files.Select(sceneFile => sceneFile.ToZenFile()).ToList();

            Log.KeyValuePair("Media Storage", $"{mediaObjects.Count} object(s)");

            foreach (var mediaObject in mediaObjects)
                Log.KeyValuePair(mediaObject.Id, $"{mediaObject.MimeType} | {mediaObject.FileSize} | {mediaObject.OriginalName}", Message.EContentType.MoreInfo);

            var mediaIds = mediaObjects.Select(o => o.Id).ToList();

            return mediaIds;
        }
    }
}