using System.IO;
using SixLabors.ImageSharp;

namespace Zen.Media.Processing
{
    public static class RasterImage
    {
        public static Image FromStream(Stream source) { return Image.Load(source); }
    }
}