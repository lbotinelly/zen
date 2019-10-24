using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Zen.Media.Processing
{
    public class ImagePackage
    {
        public IImageFormat Format;
        public Image Image;
    }

    public static class RasterImage
    {
        public static ImagePackage ToImagePackage(this Stream source)
        {
            var ret = Image.Load(source, out var format);
            return new ImagePackage { Format = format, Image = ret };
        }
    }
}