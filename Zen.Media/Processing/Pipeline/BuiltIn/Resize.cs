using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Zen.Media.Processing.Pipeline.BuiltIn
{
    public class Resize : IRasterImagePipelineItem
    {
        #region Implementation of IRasterImagePipelineItem

        public int? Width { get; set; }
        public int? Height { get; set; }

        public bool Proportional = true;

        public Image Process(Image source)
        {
            var sourceRatio = source.Width / (decimal) source.Height;

            if (Width.HasValue && !Height.HasValue) // Only width was provided
                Height = (int?) (Width / sourceRatio);

            if (!Width.HasValue && Height.HasValue) // Only height was provided
                Width = (int?) (Height * sourceRatio);

            if (Proportional) // If we're doing a proportional crop then source size may need to be adjusted.
                if (Width!= null && Height!= null)
                {
                    var projectedRatio = Width.Value / (decimal) Height.Value;

                    if (projectedRatio > sourceRatio) // Source is taller than projected; adjust height.
                        Height = (int?) (source.Height * (Width / (decimal) source.Width));
                    else Width = (int?) (source.Width * (Height / (decimal) source.Height));
                }

            // If no processing is required, just jump out.
            if (Width == source.Width && Height == source.Height) return source;

            source.Mutate(x => x.Resize(Width ?? source.Width, Height ?? source.Height));
            return source;
        }

        #endregion
    }
}