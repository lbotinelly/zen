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
        public Image SourceImage { get; set; }
        public ImagePackage SourcePackage { get; set; }
        public IImageFormat Format { get; set; }

        public Stream Process()
        {
            if (SourceStream!= null)
                if (SourceImage == null)
                {
                    SourceImage = Image.Load(SourceStream, out var format);
                    Format = format;
                }

            if (SourceImage!= null)
                if (SourcePackage == null)
                    SourcePackage = new ImagePackage { Format = Format, Image = SourceImage };

            // Now, backfills.

            SourceImage = SourceImage ?? SourcePackage.Image;
            Format = Format ?? SourcePackage.Format;

            foreach (var item in Items) SourceImage = item.Process(SourceImage);

            var memoryStream = new MemoryStream();
            SourceImage.Save(memoryStream, Format);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}