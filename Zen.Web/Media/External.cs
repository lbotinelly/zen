using System;
using System.IO;
using System.Net.Http;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Media.Processing;
using Zen.Storage.Cache;

namespace Zen.Web.Media
{
    public static class External
    {
        // private const int _MaxSize = 67108864;

        public static ImagePackage FetchImagePackage(string url = null, bool useCache = true)
        {
            if (url == null)
            {
                var query = Current.Context.Request.Query;
                if (query.ContainsKey("url")) url = query["url"];
            }

            if (url == null) return null;

            Stream stream = null;
            var logPrefix = "";
            var localCacheKey = url.Sha512Hash();

            if (useCache) stream = Local.Read(localCacheKey);

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
                    Local.Write(localCacheKey, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }

                logPrefix = "web-fetch";
            }
            else { logPrefix = "cached"; }

            var package = stream.ToImagePackage();

            // if (image.Width * image.Height > _MaxSize) throw new ArgumentException($"Combined size is invalid. Limit pixel count to {_MaxSize}, or 64Mb (width x height).");

            Log.KeyValuePair($"{logPrefix} {package.Image.Width}x{package.Image.Height} {package.Format.DefaultMimeType}", url);

            return package;
        }
    }
}