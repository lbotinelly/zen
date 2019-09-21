using System.IO;
using Microsoft.AspNetCore.Http;
using Zen.Media.Processing.Pipeline;
using Zen.Media.Processing.Pipeline.BuiltIn;

namespace Zen.Web.Media
{
    public static class Extensions
    {
        public static RasterImagePipeline ToRasterImagePipeline(this HttpRequest source, Stream stream = null, Crop.EPosition position = Crop.EPosition.Center)
        {
            var ret = new RasterImagePipeline { SourceStream = stream };

            int? w = null;
            if (source.Query.ContainsKey("w")) w = int.Parse(source.Query["w"]);

            int? h = null;
            if (source.Query.ContainsKey("h")) h = int.Parse(source.Query["h"]);

            string f = null;
            if (source.Query.ContainsKey("f")) f = source.Query["f"];

            if ((w ?? h) != null) // If [w]idth or [h]eight are defined,
            {
                ret.Items.Add(new Resize { Width = w, Height = h, Proportional = true });
                ret.Items.Add(new Crop { Width = w, Height = h, Proportional = true, Position = position });
            }

            if (f != null) { ret.Format = SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.FindFormatByFileExtension(f); }

            return ret;
        }
    }
}