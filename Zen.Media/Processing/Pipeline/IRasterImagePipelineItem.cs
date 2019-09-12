using SixLabors.ImageSharp;

namespace Zen.Media.Processing.Pipeline
{
    public interface IRasterImagePipelineItem
    {
        Image Process(Image source);
    }
}