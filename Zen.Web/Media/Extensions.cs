using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Zen.Media.Processing.Pipeline;
using Zen.Media.Processing.Pipeline.BuiltIn;

namespace Zen.Web.Media
{
    public static class Extensions
    {
        private static readonly string[] WidthParms = {"width", "w"};
        private static readonly string[] HeightParms = {"height", "h"};
        private static readonly string[] FormatParms = {"format", "f"};
        private static readonly string[] PositionParms = {"position", "p"};

        public static RasterImagePipeline ToRasterImagePipeline(this IQueryCollection source, Stream stream = null,
            Crop.EPosition position = Crop.EPosition.NotSpecified)
        {
            var ret = new RasterImagePipeline {SourceStream = stream};

            var ws = WidthParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();
            var hs = HeightParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();
            var f = FormatParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();
            var p = PositionParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();

            var w = ws != null ? int.Parse(ws) : (int?) null;
            var h = hs != null ? int.Parse(hs) : (int?) null;

            if (position == Crop.EPosition.NotSpecified)
            {
                position = Crop.EPosition.Center;

                if (p != null)
                    switch (p.ToLower())
                    {
                        case "top":
                            position = Crop.EPosition.TopCenter;
                            break;
                        case "bottom":
                            position = Crop.EPosition.BottomCenter;
                            break;
                        case "left":
                            position = Crop.EPosition.Left;
                            break;
                        case "right":
                            position = Crop.EPosition.Right;
                            break;
                        default:
                            position = Crop.EPosition.Center;
                            break;
                    }
            }

            if ((w ?? h) != null) // If [w]idth or [h]eight are defined,
            {
                ret.Items.Add(new Resize {Width = w, Height = h, Proportional = true});
                ret.Items.Add(new Crop {Width = w, Height = h, Proportional = true, Position = position});
            }

            if (f != null)
                ret.Format = SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager
                    .FindFormatByFileExtension(f);

            return ret;
        }

        public static RasterImagePipeline ToRasterImagePipeline(this HttpRequest source, Stream stream = null,
            Crop.EPosition position = Crop.EPosition.Center)
        {
            return ToRasterImagePipeline(source.Query, stream, position);
        }
    }
}