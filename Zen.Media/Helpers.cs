using System;
using System.Collections.Generic;
using System.IO;
using Zen.Media.Processing.Pipeline.BuiltIn;
using Zen.Media.Processing.Pipeline;
using Zen.Base.Extension;
using System.Net.Http;
using Zen.Media.Processing;
using Zen.Base;
using SixLabors.ImageSharp;
using Zen.Media.Model;
using static Zen.Media.Processing.RasterMedia;

namespace Zen.Media
{
    public static class Helpers
    {
        public static MediaPackage FetchMediaPackage(string url, bool useCache = true)
        {
            if (url == null) return null;

            Stream stream = null;
            var localCacheKey = url.Sha512Hash();

            if (useCache) stream = Storage.Cache.Local.Read(localCacheKey);

            string logPrefix;
            if (stream == null)
            {
                if (url.IndexOf("http", StringComparison.Ordinal) == -1) url = $"http://{url}";

                //can contain the text http and contain incorrect uri scheme.
                var isUrl = Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!isUrl) throw new ArgumentException($"Parameter is invalid: url ({url})");

                stream = new HttpClient()
                    .GetAsync(url).Result
                    .Content.ReadAsStreamAsync().Result;

                if (useCache)
                {
                    Storage.Cache.Local.Write(localCacheKey, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }

                logPrefix = "GET";
            }
            else { logPrefix = "C"; }

            var package = stream.ToMediaPackage();

            //if (package.Image.Width * package.Image.Height > _MaxSize) throw new ArgumentException($"Combined size is invalid. Limit pixel count to {_MaxSize}, or 64Mb (width x height).");

            if (package == null)
            {
                Log.KeyValuePair($"Helpers.FetchImagePackage", "No package generated.", Base.Module.Log.Message.EContentType.Warning);
                return null;
            }

            Log.KeyValuePair($"{logPrefix} {package.Image.Width}x{package.Image.Height} {package.Format}", url);

            return package;
        }



        public static RasterMediaPipeline ToRasterImagePipeline(string url, Stream stream = null, Crop.EPosition position = Crop.EPosition.NotSpecified)
        {
            var dict = new Dictionary<string, string> { ["url"] = url };

            return dict.ToRasterMediaPipeline(stream, position);
        }


        public static LocalResource GetExternalResource(this LocalResource resource)
        {
            return GetExternalResource(resource.Query, resource.CacheTag);
        }


        public static LocalResource GetExternalResource(Dictionary<string, string> dictQuery, string cacheTag = null)
        {
            var url = dictQuery["url"];

            try
            {
                if (cacheTag == null) cacheTag = dictQuery.ToJson().Sha512Hash();

                var pipeline = dictQuery.ToRasterMediaPipeline();

                if (pipeline == null)
                {
                    Zen.Base.Log.KeyValuePair("FetchImagePackage.GetExternalResource", "No pipeline generated, aborting.");
                    return null;
                }


                pipeline.SourcePackage = FetchMediaPackage(url);


                var result = pipeline.Process();


                if (result == null)
                {
                    Log.KeyValuePair("Helpers.GetExternalResource", "No pipeline received.", Base.Module.Log.Message.EContentType.Warning);
                    return null;

                }

                return new LocalResource()
                {
                    Query = dictQuery,
                    MimeType = pipeline.Format,
                    Stream = result.Stream,
                    CacheTag = cacheTag,
                    Pipeline = result
                };

            }
            catch (Exception e)
            {
                Log.KeyValuePair($"Helpers.GetExternalResource: {url}", e.Message, Base.Module.Log.Message.EContentType.Warning);
                return null;
            }


        }

        public class LocalResource
        {
            public Stream Stream { get; set; }
            public string MimeType { get; set; }
            public string CacheTag { get; set; }
            public Dictionary<string, string> Query { get; set; }
            public RasterMediaPipeline.Info Pipeline { get; internal set; }
        }

        public static LocalResource GetAndCacheExternalResource(string url, bool ignoreCached = false, bool fetchImageInfoIfCached = false)
        {
            var dict = new Dictionary<string, string> { ["url"] = url };

            return GetAndCacheExternalResource(dict, ignoreCached, fetchImageInfoIfCached);
        }

        public static LocalResource GetAndCacheExternalResource(Dictionary<string, string> dictQuery, bool ignoreCached = false, bool fetchImageInfoIfCached = false)
        {
            var cacheTag = dictQuery.ToJson().Sha512Hash();
            var mimeCacheTag = cacheTag + "_mimeType";

            LocalResource result;

            if (!ignoreCached)
            {
                var targetStream = Storage.Cache.Local.Read(cacheTag);
                var targetStreamMimeType = Storage.Cache.Local.ReadString(mimeCacheTag);

                if (targetStream != null && targetStreamMimeType != null)
                {
                    result = new LocalResource
                    {
                        Query = dictQuery,
                        CacheTag = cacheTag,
                        Stream = targetStream,
                        MimeType = targetStreamMimeType
                    };

                    if (fetchImageInfoIfCached)
                    {
                        try
                        {
                            MediaCacheData cachedData = MediaCacheData.Get(cacheTag);

                            if (cachedData == null)
                            {
                                var target = Image.Load(targetStream, out var format);
                                cachedData = new MediaCacheData() { Id = cacheTag, Width = target.Width, Height = target.Height, Success = true };
                                cachedData.Save();
                            }

                            result.Pipeline = new RasterMediaPipeline.Info
                            {
                                Width = cachedData.Width,
                                Height = cachedData.Height
                            };
                        }
                        catch (Exception e)
                        {
                            Log.Add(e, "GetAndCacheExternalResource");

                            new MediaCacheData() { Id = cacheTag, Width = 0, Height = 0, Success = false, Message = e.Message }.Save();

                            result.Pipeline = new RasterMediaPipeline.Info
                            {
                                Width = 0,
                                Height = 0
                            };
                        }
                    }

                    return result;
                }

                targetStream?.Dispose();
            }

            result = GetExternalResource(dictQuery, cacheTag);

            if (result == null)
            {
                Log.KeyValuePair("LocalResource.GetAndCacheExternalResource", "No External resource returned.", Base.Module.Log.Message.EContentType.Warning);
                return null;
            }

            if (result.MimeType == null)
            {
                Log.KeyValuePair("LocalResource.GetAndCacheExternalResource", "No MIME type detected.", Base.Module.Log.Message.EContentType.Warning);
                return null;
            }

            Storage.Cache.Local.Write(cacheTag, result.Stream);
            Storage.Cache.Local.WriteString(mimeCacheTag, result.MimeType);

            new MediaCacheData() { Id = cacheTag, Width = result.Pipeline.Width, Height = result.Pipeline.Height, Success = true }.Save();


            return result;
        }
    }
}