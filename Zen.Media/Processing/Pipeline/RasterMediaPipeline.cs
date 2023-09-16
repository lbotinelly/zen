using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static Zen.Media.Processing.RasterMedia;

namespace Zen.Media.Processing.Pipeline
{
    public class RasterMediaPipeline
    {
        public List<IRasterMediaPipelineItem> Items = new List<IRasterMediaPipelineItem>();
        public Stream SourceStream { get; set; }
        public Image SourceImage { get; set; }
        public MediaPackage SourcePackage { get; set; }
        public string Format { get; set; }

        public class Info
        {
            public MemoryStream Stream { get; set; }
            public int Height { get; internal set; }
            public int Width { get; internal set; }
        }

        public Info Process()
        {
            if (SourceStream != null)
                if (SourceImage == null)
                {
                    SourceImage = Image.Load(SourceStream, out var format);
                    Format = format.ToString();
                }

            if (SourceImage != null)
                if (SourcePackage == null)
                    SourcePackage = new MediaPackage { Format = Format.ToString(), Image = SourceImage };

            // Now, backfills.

            SourceImage = SourceImage ?? SourcePackage?.Image;
            Format = Format ?? SourcePackage?.Format;

            foreach (var item in Items) SourceImage = item.Process(SourceImage);


            var response = new Info
            {
                Stream = null,
                Height = SourceImage?.Height ?? 0,
                Width = SourceImage?.Width ?? 0,
            };


            SixLabors.ImageSharp.Formats.IImageEncoder targetEncoder = null;

            switch (Format)
            {
                case "image/png":
                    targetEncoder = new PngEncoder();
                    break;
                case "image/jpg":
                case "image/jpeg":
                    targetEncoder = new JpegEncoder();
                    break;
                case "image/gif":
                    targetEncoder = new GifEncoder();
                    break;
                default:
                    break;
            };

            if (SourceImage != null)
            {
                response.Stream = new MemoryStream();
                SourceImage?.Save(response.Stream, targetEncoder);
                response.Stream.Position = 0;
            }


            return response;
        }
    }
}