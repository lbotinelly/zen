using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Zen.Media.Processing.Pipeline.BuiltIn
{
    public class Grayscale : IRasterImagePipelineItem
    {
        #region Implementation of IRasterImagePipelineItem

        public GrayscaleMode Mode = GrayscaleMode.Bt601;

        public Image Process(Image source)
        {
            source.Mutate(x =>x.Grayscale(Mode));
            return source;
        }

        #endregion
    }
}