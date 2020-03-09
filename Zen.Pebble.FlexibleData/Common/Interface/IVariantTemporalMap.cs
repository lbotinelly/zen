using System.Collections.Generic;
using Zen.Pebble.FlexibleData.String.Localization.Interface;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariantTemporalMap<TU, T>
    {
        Dictionary<TU, IVariant<ITemporalCommented<T>>> Variants { get; set; }
    }
}