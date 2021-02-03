using System.Collections.Generic;

namespace Zen.Pebble.FlexibleData.Common.Interface
{
    public interface IVariantMap<TU, T>
    {
        Dictionary<TU, VariantList<T>> Variants { get; set; }
    }
}