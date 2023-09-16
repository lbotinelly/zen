using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Zen.Media;
using Zen.Media.Processing.Pipeline;
using Zen.Media.Processing.Pipeline.BuiltIn;

namespace Zen.Web.App.Media
{
    public static class Extensions
    {
        public static RasterImagePipeline ToRasterImagePipeline(this IQueryCollection source, Stream stream = null, Crop.EPosition cropPosition = Crop.EPosition.NotSpecified)
        {
            var dictQuery = source.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value.ToString());
            return dictQuery.ToRasterImagePipeline(stream, cropPosition);
        }

        public static RasterImagePipeline ToRasterImagePipeline(this HttpRequest source, Stream stream = null, Crop.EPosition cropPosition = Crop.EPosition.Center)
        {
            return source.Query.ToRasterImagePipeline(stream, cropPosition);
        }
    }
}