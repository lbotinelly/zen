using SixLabors.ImageSharp;

namespace Zen.Media.Processing.Pipeline
{
    public interface IRasterMediaPipelineItem
    {
        Image Process(Image source);
    }
}