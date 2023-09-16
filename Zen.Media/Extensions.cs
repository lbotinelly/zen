using System.Collections.Generic;
using System.IO;
using Zen.Media.Processing.Pipeline.BuiltIn;
using Zen.Media.Processing.Pipeline;
using System.Linq;
using Zen.Base.Extension;

namespace Zen.Media
{
    public static class Extensions
    {
        private static readonly string[] WidthParms = { "width", "w" };
        private static readonly string[] HeightParms = { "height", "h" };
        private static readonly string[] FormatParms = { "format", "f" };
        private static readonly string[] PositionParms = { "position", "p" };




        public static RasterMediaPipeline ToRasterMediaPipeline(this Dictionary<string, string> source, Stream stream = null, Crop.EPosition cropPosition = Crop.EPosition.NotSpecified)
        {
            var ret = new RasterMediaPipeline { SourceStream = stream };

            try
            {


            var ws = WidthParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();
            var hs = HeightParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();
            var f = FormatParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();
            var p = PositionParms.Where(source.ContainsKey).Select(i => source[i].ToString()).FirstOrDefault();

            var w = ws != null ? int.Parse(ws) : (int?)null;
            var h = hs != null ? int.Parse(hs) : (int?)null;

            if (cropPosition == Crop.EPosition.NotSpecified)
            {
                cropPosition = Crop.EPosition.Center;

                if (p != null)
                    switch (p.ToLower())
                    {
                        case "top":
                            cropPosition = Crop.EPosition.TopCenter;
                            break;
                        case "bottom":
                            cropPosition = Crop.EPosition.BottomCenter;
                            break;
                        case "left":
                            cropPosition = Crop.EPosition.Left;
                            break;
                        case "right":
                            cropPosition = Crop.EPosition.Right;
                            break;
                        default:
                            cropPosition = Crop.EPosition.Center;
                            break;
                    }
            }

            if ((w ?? h) != null) // If [w]idth or [h]eight are defined,
            {
                ret.Items.Add(new Resize { Width = w, Height = h, Proportional = true });
                ret.Items.Add(new Crop { Width = w, Height = h, Proportional = true, Position = cropPosition });
            }

            if (f != null)
                ret.Format = SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.FindFormatByFileExtension(f).ToString();

            return ret;

            }
            catch (System.Exception e)
            {
                Zen.Base.Log.KeyValuePair("ToRasterMediaPipeline", e.FancyString(), Base.Module.Log.Message.EContentType.Warning);
                Zen.Base.Log.Add(source.ToJson(), Base.Module.Log.Message.EContentType.Info);
                return null;
            }
        }
    }
}