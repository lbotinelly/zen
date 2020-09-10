using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Zen.Media.Processing.Pipeline.BuiltIn
{
    public class Crop : IRasterImagePipelineItem
    {
        [Flags]
        public enum EPosition
        {
            Top = 0b_0000_0001,
            HorizontalCenter = 0b_0000_0010,
            Bottom = 0b_0000_0100,
            Left = 0b_0001_0000,
            VerticalCenter = 0b_0010_0000,
            Right = 0b_0100_0000,
            Center = 0b_0010_0010,
            TopCenter = 0b_0010_0001,
            BottomCenter = 0b_0010_0100,
            NotSpecified = 0
        }

        #region Implementation of IRasterImagePipelineItem

        public int? Width { get; set; }
        public int? Height { get; set; }
        public EPosition Position { get; set; } = EPosition.Center;
        public bool Proportional { get; set; }

        public Image Process(Image source)
        {
            // Let's first determine the original image's ratio:
            var sourceRatio = source.Width / (decimal) source.Height;

            if (Width.HasValue && !Height.HasValue) // If only width was provided
                Height = (int?) (Width / sourceRatio);

            if (!Width.HasValue && Height.HasValue) // If only height was provided
                Width = (int?) (Height * sourceRatio);

            if (Width > source.Width) Width = source.Width;
            if (Height > source.Height) Height = source.Height;

            // If no processing is required, just jump out.
            if (Width == source.Width && Height == source.Height)
                return source;

            var wOffset = source.Width - Width;
            var hOffset = source.Height - Height;

            if ((Position & EPosition.Left) == EPosition.Left) wOffset = 0;
            if ((Position & EPosition.VerticalCenter) == EPosition.VerticalCenter) wOffset = wOffset / 2;

            if ((Position & EPosition.Top) == EPosition.Top) hOffset = 0;
            if ((Position & EPosition.HorizontalCenter) == EPosition.HorizontalCenter) hOffset = hOffset / 2;

            var rect = new Rectangle(wOffset.Value, hOffset.Value, Width.Value, Height.Value);

            source.Mutate(x => x.Crop(rect));
            return source;
        }

        #endregion
    }
}