using System.Collections.Generic;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariantCultured<T> : ICultured<T>
    {
        Dictionary<string, IVariant<ITemporalCommented<T>>> Variants { get; set; }
    }
}