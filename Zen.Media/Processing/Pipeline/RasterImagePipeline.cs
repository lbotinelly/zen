using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Zen.Media.Processing.Pipeline
{
    public class RasterImagePipeline
    {
        public List<IRasterImagePipelineItem> Items = new List<IRasterImagePipelineItem>();
        public Stream SourceStream { get; set; }

        public IImageFormat Format { get; set; }

        public Stream Process()
        {
            var currentImage = Image.Load(SourceStream, out var format);

            if (Format == null) Format = format;

            foreach (var item in Items) currentImage = item.Process(currentImage);

            var memoryStream = new MemoryStream();
            currentImage.Save(memoryStream, format);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}