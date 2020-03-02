using Zen.Pebble.FlexibleData.Common.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class CommentedStringValue : StringValue, ICommented<string>
    {
        public string Comments { get; set; }
    }
}