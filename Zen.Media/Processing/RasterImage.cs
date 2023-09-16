using System.IO;
using MediaInfo;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace Zen.Media.Processing
{
    public static class RasterMedia
    {
        public class MediaPackage
        {
            public string Format;
            public Image Image;
        }


        public static MediaPackage ToMediaPackage(this Stream source, ILogger logger = null)
        {
            var response = new MediaPackage();

            if (source == null) return null;

            try
            {
                if (source is FileStream)
                    if (logger != null)
                    {

                        var media = new MediaInfoWrapper(((FileStream)source).Name, logger);

                        if (media.Success)
                        {
                            var _format = MimeTypes.MimeTypeMap.GetMimeType(((FileStream)source).Name);
                            response.Format = _format;
                        }

                    }

                try
                {
                    var ret = Image.Load(source, out var format);
                    response.Format = format.DefaultMimeType;
                    response.Image = ret;
                }
                catch (System.Exception e)
                {
                }

                return response;

            }
            catch (System.Exception e)
            {
                Zen.Base.Log.KeyValuePair("RasterMedia.ToMediaPackage", "ERR " + e.Message, Base.Module.Log.Message.EContentType.Warning);
                return null;
            }
        }
    }
}