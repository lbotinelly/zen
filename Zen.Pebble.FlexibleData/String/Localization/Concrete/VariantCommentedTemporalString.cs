using System.Collections.Generic;
using Zen.Pebble.FlexibleData.Common.Interface;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.String.Localization.Concrete
{
    public class VariantCommentedTemporalString : IVariant<ITemporalCommented<string>>
    {
        public List<ITemporalCommented<string>> Variants { get; set; }
    }
}